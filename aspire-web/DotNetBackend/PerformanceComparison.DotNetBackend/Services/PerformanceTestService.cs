using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using PerformanceComparison.DotNetBackend.Models;

namespace PerformanceComparison.DotNetBackend.Services;

public class PerformanceTestService
{
    private readonly ILogger<PerformanceTestService> _logger;

    public PerformanceTestService(ILogger<PerformanceTestService> logger)
    {
        _logger = logger;
    }

    public async Task<TestResult> RunPerformanceTestAsync(
        TestConfiguration config, 
        IProgress<TestProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(true);
            var iterationTimes = new List<double>();
            var warmupSuccessful = false;

            _logger.LogInformation("Starting performance test with {Iterations} iterations", config.Iterations);

            // Create agent
            var agent = new OllamaApiClient(new Uri(config.Endpoint), config.Model)
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
                _logger.LogInformation("Warmup completed in {Time:F3} ms", warmupTimeMs);
                warmupSuccessful = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Warmup call failed");
            }

            // Run iterations
            for (int i = 0; i < config.Iterations && !cancellationToken.IsCancellationRequested; i++)
            {
                var iterationStart = Stopwatch.GetTimestamp();
                
                try
                {
                    await agent.RunAsync($"Say hello {i + 1}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Iteration {Iteration} failed", i + 1);
                }

                var iterationEnd = Stopwatch.GetTimestamp();
                var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
                iterationTimes.Add(iterationTimeMs);

                // Report progress
                if (progress != null && (i + 1) % 5 == 0)
                {
                    var testProgress = new TestProgress
                    {
                        CurrentIteration = i + 1,
                        TotalIterations = config.Iterations,
                        Status = "Running",
                        ProgressPercentage = ((i + 1) / (double)config.Iterations) * 100,
                        RecentIterationTimes = iterationTimes.TakeLast(10).ToList()
                    };
                    progress.Report(testProgress);
                }
            }

            stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0;

            var metrics = new TestMetrics
            {
                Language = "CSharp",
                Framework = "DotNet",
                Provider = "Ollama",
                Model = config.Model,
                Endpoint = config.Endpoint,
                Timestamp = DateTime.UtcNow,
                WarmupSuccessful = warmupSuccessful,
                TotalIterations = iterationTimes.Count,
                TotalExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                AverageTimePerIterationMs = iterationTimes.Average(),
                MinIterationTimeMs = iterationTimes.Min(),
                MaxIterationTimeMs = iterationTimes.Max(),
                MemoryUsedMB = memoryUsed,
                MachineInfo = GetMachineInfo()
            };

            return new TestResult
            {
                Success = true,
                Message = "Test completed successfully",
                Metrics = metrics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Performance test failed");
            return new TestResult
            {
                Success = false,
                Message = $"Test failed: {ex.Message}"
            };
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
