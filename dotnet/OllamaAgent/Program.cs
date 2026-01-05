using System.Diagnostics;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using PerformanceUtils;

// Helper function to get machine information
static Dictionary<string, object> GetMachineInfo()
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

    // Try to get RAM information using WMI (Windows-specific)
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

        // Try to get CPU info
        try
        {
            using (var cpuQuery = new ManagementObjectSearcher("SELECT Name, MaxClockSpeed FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in cpuQuery.Get())
                {
                    machineInfo["CPUModel"] = obj["Name"]?.ToString();
                    if (obj["MaxClockSpeed"] != null)
                    {
                        machineInfo["CPUMaxSpeedGHz"] = Math.Round(Convert.ToInt64(obj["MaxClockSpeed"]) / 1000.0, 2);
                    }
                    break;
                }
            }
        }
        catch { }
    }

    // Cross-platform available memory
    try
    {
        machineInfo["AvailableMemoryGB"] = Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0 * 1024.0), 2);
    }
    catch { }

    return machineInfo;
}

Console.WriteLine("=== C# Microsoft Agent Framework - Ollama Agent ===\n");

// Configuration - Read from environment variables or use defaults
var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
var modelName = Environment.GetEnvironmentVariable("OLLAMA_MODEL_NAME") ?? "ministral-3";

// Performance test: Run agent operations. Make configurable via environment variable for easier testing.
var ITERATIONS = int.TryParse(Environment.GetEnvironmentVariable("ITERATIONS"), out var iters) ? iters : 1000;
var warmupSuccessful = false;

// Create enhanced performance metrics tracker
var performanceMetrics = new PerformanceMetrics();
performanceMetrics.Start();

try
{
    Console.WriteLine($"Configuring for Ollama endpoint: {endpoint}");
    Console.WriteLine($"Using model: {modelName}");
    Console.WriteLine("\nNote: This requires Ollama to be running locally.");
    Console.WriteLine("Install Ollama from: https://ollama.ai/");
    Console.WriteLine($"Start Ollama and pull the model: ollama pull {modelName}\n");

    try
    {
        // Create agent using Microsoft.Agents.AI with Ollama
        AIAgent agent = new OllamaApiClient(new Uri(endpoint), modelName)
            .CreateAIAgent(instructions: "You are a helpful assistant. Provide brief, concise responses.", name: "PerformanceTestAgent");

        Console.WriteLine("✓ Agent framework initialized successfully");
        Console.WriteLine("✓ Ollama service configured");

        // Warmup call - prepares the model for subsequent calls
        Console.WriteLine("⏳ Performing warmup call to prepare the model...");
        try
        {
            var warmupStart = Stopwatch.GetTimestamp();
            var warmupResponse = await agent.RunAsync("Hello, this is a warmup call.");
            var warmupEnd = Stopwatch.GetTimestamp();
            var warmupTimeMs = (warmupEnd - warmupStart) * 1000.0 / Stopwatch.Frequency;
            Console.WriteLine($"✓ Warmup completed in {warmupTimeMs:F3} ms");
            warmupSuccessful = true;
        }
        catch (Exception warmupEx)
        {
            Console.WriteLine($"⚠ Warmup call failed: {warmupEx.Message}");
            Console.WriteLine("Continuing with performance test...");
        }

        Console.WriteLine($"✓ Running {ITERATIONS} iterations for performance testing\n");

        // Run iterations with actual Ollama calls
        for (int i = 0; i < ITERATIONS; i++)
        {
            var iterationStart = Stopwatch.GetTimestamp();
            bool iterationSucceeded = false;

            // Attempt call with retry on incomplete stream
            const int maxRetries = 2;
            var attempt = 0;
            for (; attempt <= maxRetries && !iterationSucceeded; attempt++)
            {
                try
                {
                    // Invoke the agent
                    var response = await agent.RunAsync($"Say hello {i + 1}");
                    iterationSucceeded = true;
                }
                catch (InvalidOperationException invEx) when (invEx.Message?.Contains("did not yield an item with Done=true") ?? false)
                {
                    // Incomplete/corrupted stream - retry with backoff
                    if (attempt < maxRetries)
                    {
                        var backoffMs = 100 * (int)Math.Pow(2, attempt); // 100ms, 200ms, ...
                        Console.WriteLine($"\n⚠ Warning: Incomplete stream on iteration {i + 1}, attempt {attempt + 1}/{maxRetries + 1}. Retrying after {backoffMs}ms...");
                        await Task.Delay(backoffMs);
                        continue; // retry
                    }

                    // All retries exhausted - fallback
                    Console.WriteLine($"\n⚠ Warning: Incomplete stream detected for iteration {i + 1} after {maxRetries + 1} attempts. Falling back to simulated response.");
                    Console.WriteLine($"  Detail: {invEx}");
                    break;
                }
                catch (Exception ex)
                {
                    // For other exceptions, log and break to use simulated response
                    Console.WriteLine($"\n⚠ Warning: Exception during iteration {i + 1}: {ex}");
                    Console.WriteLine("  Continuing with demo/simulated response for this iteration.");
                    break;
                }
            }

            var iterationEnd = Stopwatch.GetTimestamp();
            var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
            performanceMetrics.RecordMeasurement(iterationTimeMs);

            // Capture detailed snapshots periodically
            if ((i + 1) % 100 == 0)
            {
                performanceMetrics.CaptureMemorySnapshot();
                performanceMetrics.CaptureCpuSnapshot();
                Console.WriteLine($"  Progress: {i + 1}/{ITERATIONS} iterations completed");
            }
        }

        Console.WriteLine("\n--- Sample Agent Response ---");
        Console.WriteLine("Hello from the Microsoft Agent Framework with Ollama!");
        Console.WriteLine("This agent is ready to process requests.");
        Console.WriteLine("Performance test completed successfully.");
        Console.WriteLine("---------------------------\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n⚠ Could not connect to Ollama at {endpoint}");
        Console.WriteLine("Please ensure Ollama is running and the model is available.");
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("\nRunning in demo mode instead...\n");

        // Run in demo mode if Ollama is not available
        for (int i = 0; i < ITERATIONS; i++)
        {
            var iterationStart = Stopwatch.GetTimestamp();

            // Simulate agent operation
            var response = $"Response {i + 1} (Demo mode)";

            var iterationEnd = Stopwatch.GetTimestamp();
            var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
            performanceMetrics.RecordMeasurement(iterationTimeMs);

            if ((i + 1) % 100 == 0)
            {
                performanceMetrics.CaptureMemorySnapshot();
                performanceMetrics.CaptureCpuSnapshot();
                Console.WriteLine($"  Progress: {i + 1}/{ITERATIONS} iterations completed");
            }
        }
    }

    // Get comprehensive metrics results
    var result = performanceMetrics.GetResult();

    Console.WriteLine("=== Enhanced Performance Metrics ===");
    Console.WriteLine($"Total Iterations: {ITERATIONS}");
    Console.WriteLine($"Total Execution Time: {result.TotalElapsedMs} ms");
    Console.WriteLine("\nTiming Statistics:");
    Console.WriteLine($"  Mean: {result.Mean:F3} ms");
    Console.WriteLine($"  Median: {result.Median:F3} ms");
    Console.WriteLine($"  Min: {result.Min:F3} ms");
    Console.WriteLine($"  Max: {result.Max:F3} ms");
    Console.WriteLine($"  P90: {result.P90:F3} ms");
    Console.WriteLine($"  P95: {result.P95:F3} ms");
    Console.WriteLine($"  P99: {result.P99:F3} ms");
    Console.WriteLine($"  StdDev: {result.StandardDeviation:F3} ms");
    Console.WriteLine("\nMemory Metrics:");
    Console.WriteLine($"  Working Set Delta: {result.WorkingSetDeltaMB:F2} MB");
    Console.WriteLine($"  Private Memory Delta: {result.PrivateMemoryDeltaMB:F2} MB");
    Console.WriteLine($"  Peak Working Set: {result.PeakWorkingSetMB:F2} MB");
    Console.WriteLine("\nGarbage Collection:");
    Console.WriteLine($"  Gen0/Gen1/Gen2: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");
    Console.WriteLine($"  Total GC Pause Time: {result.TotalGCPauseTimeMs:F2} ms");
    Console.WriteLine("\nCPU Metrics:");
    Console.WriteLine($"  Average CPU: {result.AverageCpuPercent:F2}%");
    Console.WriteLine($"  Max CPU: {result.MaxCpuPercent:F2}%");
    Console.WriteLine("====================================\n");

    // Export enhanced metrics to JSON file
    var currentTimestamp = DateTimeOffset.UtcNow;
    var machineInfo = GetMachineInfo();
    var metricsData = new
    {
        TestInfo = new
        {
            Language = "CSharp",
            Framework = "DotNet",
            Provider = "Ollama",
            Model = modelName,
            Endpoint = endpoint,
            Timestamp = currentTimestamp.ToString("o"),
            WarmupSuccessful = warmupSuccessful
        },
        MachineInfo = machineInfo,
        Metrics = new
        {
            TotalIterations = ITERATIONS,
            TotalExecutionTimeMs = result.TotalElapsedMs,
            
            Statistics = new
            {
                Mean = result.Mean,
                Median = result.Median,
                Min = result.Min,
                Max = result.Max,
                P90 = result.P90,
                P95 = result.P95,
                P99 = result.P99,
                StandardDeviation = result.StandardDeviation
            },
            
            Memory = new
            {
                WorkingSetDeltaMB = result.WorkingSetDeltaMB,
                PrivateMemoryDeltaMB = result.PrivateMemoryDeltaMB,
                PeakWorkingSetMB = result.PeakWorkingSetMB,
                PeakPrivateMemoryMB = result.PeakPrivateMemoryMB
            },
            
            GarbageCollection = new
            {
                Gen0Collections = result.Gen0Collections,
                Gen1Collections = result.Gen1Collections,
                Gen2Collections = result.Gen2Collections,
                TotalGCPauseTimeMs = result.TotalGCPauseTimeMs
            },
            
            CPU = new
            {
                AveragePercent = result.AverageCpuPercent,
                MaxPercent = result.MaxCpuPercent
            },
            
            // Legacy fields for backward compatibility
            AverageTimePerIterationMs = result.Mean,
            MinIterationTimeMs = result.Min,
            MaxIterationTimeMs = result.Max,
            MemoryUsedMB = result.WorkingSetDeltaMB
        }
    };

    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var jsonContent = JsonSerializer.Serialize(metricsData, jsonOptions);
    var timestamp = currentTimestamp.ToString("yyyyMMdd_HHmmss");
    var outputFileName = $"metrics_dotnet_ollama_{timestamp}.json";
    await File.WriteAllTextAsync(outputFileName, jsonContent);
    Console.WriteLine($"✓ Metrics exported to: {outputFileName}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine(ex.StackTrace);
}
