using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Management;
using System.Collections.Concurrent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using PerformanceComparison.DotNetBackend.Models;
using System.Text.Json;

namespace PerformanceComparison.DotNetBackend.Services;

public class BackgroundTestService
{
    private readonly ILogger<BackgroundTestService> _logger;
    private readonly ConcurrentDictionary<string, TestSession> _sessions = new();
    private CancellationTokenSource? _currentCts;
    private Task? _currentTask;
    private readonly object _lock = new();

    public BackgroundTestService(ILogger<BackgroundTestService> logger)
    {
        _logger = logger;
    }

    public string StartTest(TestConfiguration config)
    {
        lock (_lock)
        {
            // Stop any existing test
            StopTest();

            var sessionId = Guid.NewGuid().ToString();
            _currentCts = new CancellationTokenSource();

            var session = new TestSession
            {
                SessionId = sessionId,
                Configuration = config,
                Status = "Running",
                StartTime = DateTime.UtcNow,
                CurrentIteration = 0,
                TotalIterations = config.Iterations,
                IterationTimes = new List<double>()
            };

            _sessions[sessionId] = session;

            _currentTask = Task.Run(async () => await ExecuteTestAsync(session, _currentCts.Token), _currentCts.Token);

            return sessionId;
        }
    }

    public bool StopTest()
    {
        lock (_lock)
        {
            if (_currentCts != null && !_currentCts.IsCancellationRequested)
            {
                _currentCts.Cancel();
                _currentCts = null;

                // Update session status
                foreach (var session in _sessions.Values.Where(s => s.Status == "Running"))
                {
                    session.Status = "Stopped";
                }

                return true;
            }
            return false;
        }
    }

    public TestSession? GetStatus(string? sessionId = null)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            // Return the most recent session
            return _sessions.Values.OrderByDescending(s => s.StartTime).FirstOrDefault();
        }

        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    private async Task ExecuteTestAsync(TestSession session, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(true);

            _logger.LogInformation("Starting background test with {Iterations} iterations", session.Configuration.Iterations);

            // Reset session metrics explicitly when a test starts
            session.CurrentIteration = 0;
            session.IterationTimes = new List<double>();
            session.ElapsedTimeMs = 0;
            session.WarmupSuccessful = false;
            session.WarmupTimeMs = 0;
            session.LastIterationTimeMs = 0;
            session.SuccessCount = 0;
            session.FailureCount = 0;
            session.IterationsPerSecond = 0;
            session.EstimatedTimeRemainingMs = 0;

            // Create agent
            var agent = new OllamaApiClient(new Uri(session.Configuration.Endpoint), session.Configuration.Model)
                .CreateAIAgent(
                    instructions: "You are a helpful assistant. Provide brief, concise responses.",
                    name: "PerformanceTestAgent");

            // Warmup call
            try
            {
                var warmupStart = Stopwatch.GetTimestamp();
                await agent.RunAsync("Hello, this is a warmup call.");
                var warmupEnd = Stopwatch.GetTimestamp();
                var warmupTimeMs = (warmupEnd - warmupStart) * 1000.0 / Stopwatch.Frequency;
                session.WarmupSuccessful = true;
                session.WarmupTimeMs = warmupTimeMs;
                _logger.LogInformation("Warmup completed in {Time:F3} ms", warmupTimeMs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Warmup call failed");
            }

            // Run iterations
            for (int i = 0; i < session.Configuration.Iterations && !cancellationToken.IsCancellationRequested; i++)
            {
                var iterationStart = Stopwatch.GetTimestamp();
                bool success = false;
                try
                {
                    await agent.RunAsync($"Say hello {i + 1}");
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Iteration {Iteration} failed", i + 1);
                    success = false;
                }

                var iterationEnd = Stopwatch.GetTimestamp();
                var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;

                session.IterationTimes.Add(iterationTimeMs);
                session.CurrentIteration = i + 1;
                session.LastIterationTimeMs = iterationTimeMs;
                session.ElapsedTimeMs = stopwatch.ElapsedMilliseconds;
                if (success) session.SuccessCount++; else session.FailureCount++;

                // Compute iterations per second as a rolling metric
                var totalSec = Math.Max(1, stopwatch.ElapsedMilliseconds) / 1000.0;
                session.IterationsPerSecond = session.CurrentIteration / totalSec;
                // Estimate remaining time (ms)
                var avg = session.IterationTimes.Any() ? session.IterationTimes.Average() : 0;
                session.EstimatedTimeRemainingMs = (session.Configuration.Iterations - session.CurrentIteration) * avg;
            }

            stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0;

            session.Status = cancellationToken.IsCancellationRequested ? "Stopped" : "Completed";
            session.ElapsedTimeMs = stopwatch.ElapsedMilliseconds;
            session.MemoryUsedMB = memoryUsed;
            session.MachineInfo = GetMachineInfo();

            // Export metrics JSON file (optional - helps with consistency across repo)
            try
            {
                var metricsData = new
                {
                    TestInfo = new
                    {
                        Language = "CSharp",
                        Framework = "DotNet",
                        Provider = "Ollama",
                        Model = session.Configuration.Model,
                        Endpoint = session.Configuration.Endpoint,
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        WarmupSuccessful = session.WarmupSuccessful,
                        WarmupTimeMs = session.WarmupTimeMs
                    },
                    MachineInfo = session.MachineInfo,
                    Metrics = new
                    {
                        TotalIterations = session.TotalIterations,
                        TotalExecutionTimeMs = session.ElapsedTimeMs,
                        AverageTimePerIterationMs = session.IterationTimes.Any() ? session.IterationTimes.Average() : 0,
                        MinIterationTimeMs = session.IterationTimes.Any() ? session.IterationTimes.Min() : 0,
                        MaxIterationTimeMs = session.IterationTimes.Any() ? session.IterationTimes.Max() : 0,
                        MemoryUsedMB = session.MemoryUsedMB,
                        SuccessCount = session.SuccessCount,
                        FailureCount = session.FailureCount
                    }
                };

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(metricsData, jsonOptions);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var filename = $"metrics_dotnet_ollama_{stamp}.json";
                File.WriteAllText(filename, json);
                _logger.LogInformation("Metrics exported to {File}", filename);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to export metrics JSON");
            }

            _logger.LogInformation("Test completed. Status: {Status}, Iterations: {Count}", session.Status, session.IterationTimes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background test failed");
            session.Status = "Failed";
            session.ErrorMessage = ex.Message;
        }
    }

    private static Dictionary<string, object> GetMachineInfo()
    {
        var machineInfo = new Dictionary<string, object>();

        try
        {
            machineInfo["OSDescription"] = RuntimeInformation.OSDescription;
            machineInfo["ProcessorCount"] = Environment.ProcessorCount;
            machineInfo["Architecture"] = RuntimeInformation.ProcessArchitecture.ToString();
            machineInfo["DotNetVersion"] = RuntimeInformation.FrameworkDescription;
        }
        catch { }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                using (var memQuery = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in memQuery.Get())
                    {
                        var totalRamMb = Convert.ToInt64(obj["TotalVisibleMemorySize"]) / 1024;
                        machineInfo["TotalMemoryGB"] = Math.Round(totalRamMb / 1024.0, 2);
                        break;
                    }
                }
            }
            catch { }
        }

        return machineInfo;
    }
}

public class TestSession
{
    public string SessionId { get; set; } = string.Empty;
    public TestConfiguration Configuration { get; set; } = new();
    public string Status { get; set; } = "Idle";
    public DateTime StartTime { get; set; }
    public int CurrentIteration { get; set; }
    public int TotalIterations { get; set; }
    public long ElapsedTimeMs { get; set; }
    public List<double> IterationTimes { get; set; } = new();
    public bool WarmupSuccessful { get; set; }
    // New status metrics
    public double WarmupTimeMs { get; set; }
    public double LastIterationTimeMs { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double IterationsPerSecond { get; set; }
    public double EstimatedTimeRemainingMs { get; set; }
    public double MemoryUsedMB { get; set; }
    public Dictionary<string, object>? MachineInfo { get; set; }
    public string? ErrorMessage { get; set; }
}
