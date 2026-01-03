using System.Diagnostics;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

Console.WriteLine("=== C# Microsoft Agent Framework - Azure OpenAI Agent ===\n");

// Configuration - Read from environment variables
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";
var testMode = Environment.GetEnvironmentVariable("TEST_MODE") ?? "standard";
var ITERATIONS = int.TryParse(Environment.GetEnvironmentVariable("ITERATIONS"), out var iters) ? iters : 1000;
var BATCH_SIZE = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out var batchSize) ? batchSize : 10;
var CONCURRENT_REQUESTS = int.TryParse(Environment.GetEnvironmentVariable("CONCURRENT_REQUESTS"), out var concReq) ? concReq : 5;

// Comprehensive benchmarking scenarios
var benchmarkScenarios = new Dictionary<string, string>
{
    ["simple"] = "Say hello",
    ["medium"] = "Explain what an AI agent is in one sentence",
    ["long_output"] = "Write a detailed paragraph about the benefits of cloud computing",
    ["reasoning"] = "If you have 3 apples and buy 2 more, then give away 1, how many do you have? Explain your reasoning",
    ["conceptual"] = "What is the difference between machine learning and deep learning?"
};

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);
var process = Process.GetCurrentProcess();
var startCpuTime = process.TotalProcessorTime;
var startWallTime = DateTime.UtcNow;

var iterationTimes = new List<double>();
var cpuSamples = new List<double>();
var timeToFirstTokens = new List<double>();
var scenarioResults = new Dictionary<string, List<double>>();
var warmupSuccessful = false;
AIAgent? agent = null;

try
{
    if (string.IsNullOrEmpty(endpoint))
    {
        Console.WriteLine("⚠ AZURE_OPENAI_ENDPOINT not found. Using demo mode.");
        Console.WriteLine("To use Azure OpenAI, set the following environment variables:");
        Console.WriteLine("  - AZURE_OPENAI_ENDPOINT");
        Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, defaults to 'gpt-5-mini')");
        Console.WriteLine($"\n✓ Agent framework structure ready (demo mode)");
        Console.WriteLine($"✓ Test mode: {testMode}");
        Console.WriteLine($"✓ Running {ITERATIONS} iterations for performance testing\n");

        // Run in demo mode
        await RunDemoMode(testMode, ITERATIONS, BATCH_SIZE, CONCURRENT_REQUESTS, 
            benchmarkScenarios, iterationTimes, cpuSamples, timeToFirstTokens, scenarioResults);
    }
    else
    {
        // Create agent using Microsoft.Agents.AI with Azure OpenAI
        agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new AzureCliCredential())
            .GetChatClient(deploymentName)
            .CreateAIAgent(instructions: "You are a helpful assistant. Provide brief, concise responses.", name: "PerformanceTestAgent");

        Console.WriteLine("✓ Agent framework initialized successfully");
        Console.WriteLine("✓ Azure OpenAI service configured");
        Console.WriteLine($"✓ Using deployment: {deploymentName}");
        Console.WriteLine($"✓ Test mode: {testMode}");
        
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

        // Run actual tests with agent
        switch (testMode.ToLower())
        {
            case "batch":
                Console.WriteLine($"Running in BATCH mode with batch size: {BATCH_SIZE}\n");
                await RunBatchTest(agent, ITERATIONS, BATCH_SIZE, iterationTimes, cpuSamples);
                break;
                
            case "concurrent":
                Console.WriteLine($"Running in CONCURRENT mode with {CONCURRENT_REQUESTS} concurrent requests\n");
                await RunConcurrentTest(agent, ITERATIONS, CONCURRENT_REQUESTS, iterationTimes, cpuSamples);
                break;
                
            case "streaming":
                Console.WriteLine("Running in STREAMING mode with time-to-first-token measurement\n");
                await RunStreamingTest(agent, ITERATIONS, iterationTimes, timeToFirstTokens, cpuSamples);
                break;
                
            case "scenarios":
                Console.WriteLine("Running COMPREHENSIVE SCENARIOS test\n");
                await RunScenariosTest(agent, benchmarkScenarios, iterationTimes, scenarioResults, cpuSamples);
                break;
                
            default:
                Console.WriteLine("Running in STANDARD mode\n");
                await RunStandardTest(agent, ITERATIONS, iterationTimes, cpuSamples);
                break;
        }

        // Show a sample streaming response
        if (testMode.ToLower() != "streaming")
        {
            Console.WriteLine("\n--- Sample Agent Streaming Response ---");
            await foreach (var update in agent.RunStreamingAsync("Tell me a brief joke about a pirate."))
            {
                Console.Write(update);
            }
            Console.WriteLine("\n---------------------------\n");
        }
    }

    // Calculate statistics
    var avgIterationTime = iterationTimes.Average();
    var minIterationTime = iterationTimes.Min();
    var maxIterationTime = iterationTimes.Max();
    var avgCpu = cpuSamples.Count > 0 ? cpuSamples.Average() : 0;

    // Stop performance measurement
    stopwatch.Stop();
    var endMemory = GC.GetTotalMemory(false);
    var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0;

    Console.WriteLine("=== Performance Metrics ===");
    Console.WriteLine($"Total Iterations: {iterationTimes.Count}");
    Console.WriteLine($"Total Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Average Time per Iteration: {avgIterationTime:F3} ms");
    Console.WriteLine($"Min Iteration Time: {minIterationTime:F3} ms");
    Console.WriteLine($"Max Iteration Time: {maxIterationTime:F3} ms");
    Console.WriteLine($"Memory Used: {memoryUsed:F2} MB");
    Console.WriteLine($"Average CPU Usage: {avgCpu:F2}%");
    
    if (timeToFirstTokens.Count > 0)
    {
        Console.WriteLine($"Average Time to First Token: {timeToFirstTokens.Average():F3} ms");
    }
    
    Console.WriteLine("========================\n");
    
    // Export comprehensive metrics
    await ExportMetrics(testMode, deploymentName, endpoint, warmupSuccessful, stopwatch.ElapsedMilliseconds, 
        iterationTimes, memoryUsed, avgCpu, timeToFirstTokens, scenarioResults, BATCH_SIZE, CONCURRENT_REQUESTS);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine(ex.StackTrace);
}

async Task RunDemoMode(string mode, int iterations, int batchSize, int concurrentReq, 
    Dictionary<string, string> scenarios, List<double> times, List<double> cpuSamples, 
    List<double> ttfts, Dictionary<string, List<double>> scenarioResults)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    
    switch (mode.ToLower())
    {
        case "batch":
            Console.WriteLine($"Running in BATCH mode with batch size: {batchSize}\n");
            for (int batch = 0; batch < (iterations + batchSize - 1) / batchSize; batch++)
            {
                var currentBatchSize = Math.Min(batchSize, iterations - batch * batchSize);
                for (int i = 0; i < currentBatchSize; i++)
                {
                    var start = Stopwatch.GetTimestamp();
                    var response = $"Demo response {batch * batchSize + i + 1}";
                    var end = Stopwatch.GetTimestamp();
                    times.Add((end - start) * 1000.0 / Stopwatch.Frequency);
                }
                
                var currentCpuTime = process.TotalProcessorTime;
                var currentWallTime = DateTime.UtcNow;
                var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
                var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
                cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
                lastCpuTime = currentCpuTime;
                lastWallTime = currentWallTime;
                Console.WriteLine($"  Batch {batch + 1} completed");
            }
            break;
            
        case "scenarios":
            Console.WriteLine("Running COMPREHENSIVE SCENARIOS test\n");
            foreach (var scenario in scenarios)
            {
                Console.WriteLine($"Running scenario: {scenario.Key}");
                var scenarioTimes = new List<double>();
                for (int i = 0; i < 200; i++)
                {
                    var start = Stopwatch.GetTimestamp();
                    var response = $"Demo response to: {scenario.Value}";
                    var end = Stopwatch.GetTimestamp();
                    var time = (end - start) * 1000.0 / Stopwatch.Frequency;
                    scenarioTimes.Add(time);
                    times.Add(time);
                }
                scenarioResults[scenario.Key] = scenarioTimes;
                Console.WriteLine($"  Completed {scenario.Key}: avg {scenarioTimes.Average():F3} ms\n");
            }
            break;
            
        default:
            for (int i = 0; i < iterations; i++)
            {
                var start = Stopwatch.GetTimestamp();
                var response = $"Demo response {i + 1}";
                var end = Stopwatch.GetTimestamp();
                times.Add((end - start) * 1000.0 / Stopwatch.Frequency);
                
                if ((i + 1) % 100 == 0)
                {
                    Console.WriteLine($"  Progress: {i + 1}/{iterations} iterations completed");
                }
            }
            break;
    }
}

async Task RunStandardTest(AIAgent agent, int iterations, List<double> times, List<double> cpuSamples)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    
    for (int i = 0; i < iterations; i++)
    {
        var iterationStart = Stopwatch.GetTimestamp();
        var response = await agent.RunAsync($"Say hello {i + 1}");
        var iterationEnd = Stopwatch.GetTimestamp();
        times.Add((iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency);
        
        if ((i + 1) % 100 == 0)
        {
            var currentCpuTime = process.TotalProcessorTime;
            var currentWallTime = DateTime.UtcNow;
            var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
            var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
            cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
            lastCpuTime = currentCpuTime;
            lastWallTime = currentWallTime;
            Console.WriteLine($"  Progress: {i + 1}/{iterations} iterations completed");
        }
    }
}

async Task RunBatchTest(AIAgent agent, int iterations, int batchSize, List<double> times, List<double> cpuSamples)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    var batches = (iterations + batchSize - 1) / batchSize;
    
    for (int batch = 0; batch < batches; batch++)
    {
        var batchStart = Stopwatch.GetTimestamp();
        var currentBatchSize = Math.Min(batchSize, iterations - batch * batchSize);
        
        var batchTasks = new List<Task>();
        for (int i = 0; i < currentBatchSize; i++)
        {
            int idx = batch * batchSize + i;
            batchTasks.Add(agent.RunAsync($"Request {idx + 1}"));
        }
        await Task.WhenAll(batchTasks);
        
        var batchEnd = Stopwatch.GetTimestamp();
        var batchTimeMs = (batchEnd - batchStart) * 1000.0 / Stopwatch.Frequency;
        var avgBatchTime = batchTimeMs / currentBatchSize;
        
        for (int i = 0; i < currentBatchSize; i++)
        {
            times.Add(avgBatchTime);
        }
        
        var currentCpuTime = process.TotalProcessorTime;
        var currentWallTime = DateTime.UtcNow;
        var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
        var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
        cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
        lastCpuTime = currentCpuTime;
        lastWallTime = currentWallTime;
        Console.WriteLine($"  Batch {batch + 1}/{batches} completed ({currentBatchSize} items in {batchTimeMs:F3} ms)");
    }
}

async Task RunConcurrentTest(AIAgent agent, int iterations, int concurrentReq, List<double> times, List<double> cpuSamples)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    var groups = (iterations + concurrentReq - 1) / concurrentReq;
    
    for (int group = 0; group < groups; group++)
    {
        var groupStart = Stopwatch.GetTimestamp();
        var currentGroupSize = Math.Min(concurrentReq, iterations - group * concurrentReq);
        
        var tasks = new List<Task<double>>();
        for (int i = 0; i < currentGroupSize; i++)
        {
            int requestId = group * concurrentReq + i;
            tasks.Add(Task.Run(async () =>
            {
                var start = Stopwatch.GetTimestamp();
                await agent.RunAsync($"Concurrent request {requestId + 1}");
                var end = Stopwatch.GetTimestamp();
                return (end - start) * 1000.0 / Stopwatch.Frequency;
            }));
        }
        
        var results = await Task.WhenAll(tasks);
        times.AddRange(results);
        
        var groupEnd = Stopwatch.GetTimestamp();
        var groupTimeMs = (groupEnd - groupStart) * 1000.0 / Stopwatch.Frequency;
        
        var currentCpuTime = process.TotalProcessorTime;
        var currentWallTime = DateTime.UtcNow;
        var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
        var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
        cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
        lastCpuTime = currentCpuTime;
        lastWallTime = currentWallTime;
        Console.WriteLine($"  Group {group + 1}/{groups} completed ({currentGroupSize} concurrent requests in {groupTimeMs:F3} ms)");
    }
}

async Task RunStreamingTest(AIAgent agent, int iterations, List<double> times, List<double> ttfts, List<double> cpuSamples)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    
    for (int i = 0; i < iterations; i++)
    {
        var iterationStart = Stopwatch.GetTimestamp();
        var firstToken = false;
        var firstTokenTime = 0L;
        
        await foreach (var chunk in agent.RunStreamingAsync($"Say hello {i + 1}"))
        {
            if (!firstToken && chunk != null)
            {
                firstTokenTime = Stopwatch.GetTimestamp();
                ttfts.Add((firstTokenTime - iterationStart) * 1000.0 / Stopwatch.Frequency);
                firstToken = true;
            }
        }
        
        var iterationEnd = Stopwatch.GetTimestamp();
        times.Add((iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency);
        
        if ((i + 1) % 100 == 0)
        {
            var currentCpuTime = process.TotalProcessorTime;
            var currentWallTime = DateTime.UtcNow;
            var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
            var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
            cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
            lastCpuTime = currentCpuTime;
            lastWallTime = currentWallTime;
            Console.WriteLine($"  Progress: {i + 1}/{iterations} iterations completed");
        }
    }
}

async Task RunScenariosTest(AIAgent agent, Dictionary<string, string> scenarios, List<double> times, 
    Dictionary<string, List<double>> scenarioResults, List<double> cpuSamples)
{
    var lastCpuTime = process.TotalProcessorTime;
    var lastWallTime = DateTime.UtcNow;
    
    foreach (var scenario in scenarios)
    {
        Console.WriteLine($"Running scenario: {scenario.Key}");
        var scenarioTimes = new List<double>();
        
        for (int i = 0; i < 200; i++)
        {
            var iterationStart = Stopwatch.GetTimestamp();
            var response = await agent.RunAsync(scenario.Value);
            var iterationEnd = Stopwatch.GetTimestamp();
            var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
            scenarioTimes.Add(iterationTimeMs);
            times.Add(iterationTimeMs);
        }
        
        scenarioResults[scenario.Key] = scenarioTimes;
        var currentCpuTime = process.TotalProcessorTime;
        var currentWallTime = DateTime.UtcNow;
        var cpuUsed = (currentCpuTime - lastCpuTime).TotalMilliseconds;
        var wallTime = (currentWallTime - lastWallTime).TotalMilliseconds;
        cpuSamples.Add((cpuUsed / (wallTime * Environment.ProcessorCount)) * 100);
        lastCpuTime = currentCpuTime;
        lastWallTime = currentWallTime;
        Console.WriteLine($"  Completed {scenario.Key}: avg {scenarioTimes.Average():F3} ms\n");
    }
}

async Task ExportMetrics(string testMode, string model, string? endpoint, bool warmupSuccess, 
    long totalTimeMs, List<double> iterationTimes, double memoryUsed, double avgCpu, 
    List<double> ttfts, Dictionary<string, List<double>> scenarios, int batchSize, int concurrentReq)
{
    var currentTimestamp = DateTimeOffset.UtcNow;
    
    var metricsData = new
    {
        TestInfo = new
        {
            Language = "CSharp",
            Framework = "DotNet",
            Provider = "AzureOpenAI",
            Model = model,
            Endpoint = endpoint ?? "N/A (Demo Mode)",
            TestMode = testMode,
            Timestamp = currentTimestamp.ToString("o"),
            WarmupSuccessful = warmupSuccess
        },
        Configuration = new
        {
            BatchSize = batchSize,
            ConcurrentRequests = concurrentReq
        },
        Metrics = new
        {
            TotalIterations = iterationTimes.Count,
            TotalExecutionTimeMs = totalTimeMs,
            AverageTimePerIterationMs = iterationTimes.Average(),
            MinIterationTimeMs = iterationTimes.Min(),
            MaxIterationTimeMs = iterationTimes.Max(),
            MedianIterationTimeMs = GetMedian(iterationTimes),
            StandardDeviationMs = GetStandardDeviation(iterationTimes),
            MemoryUsedMB = memoryUsed,
            AverageCpuUsagePercent = avgCpu,
            TimeToFirstTokenMs = ttfts.Count > 0 ? ttfts.Average() : (double?)null,
            ScenarioResults = scenarios.Count > 0 ? scenarios.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    AverageMs = kvp.Value.Average(),
                    MinMs = kvp.Value.Min(),
                    MaxMs = kvp.Value.Max(),
                    MedianMs = GetMedian(kvp.Value)
                }
            ) : null
        },
        Summary = GenerateSummary(testMode, iterationTimes, memoryUsed, avgCpu, ttfts, scenarios)
    };

    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var jsonContent = JsonSerializer.Serialize(metricsData, jsonOptions);
    var timestamp = currentTimestamp.ToString("yyyyMMdd_HHmmss");
    var outputFileName = $"metrics_dotnet_azureopenai_{testMode}_{timestamp}.json";
    await File.WriteAllTextAsync(outputFileName, jsonContent);
    Console.WriteLine($"✓ Metrics exported to: {outputFileName}\n");
}

double GetMedian(List<double> values)
{
    var sorted = values.OrderBy(x => x).ToList();
    int mid = sorted.Count / 2;
    return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
}

double GetStandardDeviation(List<double> values)
{
    var avg = values.Average();
    var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
    return Math.Sqrt(sumOfSquares / values.Count);
}

string GenerateSummary(string testMode, List<double> times, double memory, double cpu, 
    List<double> ttfts, Dictionary<string, List<double>> scenarios)
{
    var summary = $"Test completed in {testMode} mode. ";
    summary += $"Processed {times.Count} iterations with average latency of {times.Average():F3}ms. ";
    summary += $"Memory usage: {memory:F2}MB, CPU usage: {cpu:F2}%. ";
    
    if (ttfts.Count > 0)
    {
        summary += $"Average time to first token: {ttfts.Average():F3}ms. ";
    }
    
    if (scenarios.Count > 0)
    {
        var fastest = scenarios.MinBy(s => s.Value.Average());
        var slowest = scenarios.MaxBy(s => s.Value.Average());
        summary += $"Fastest scenario: {fastest.Key} ({fastest.Value.Average():F3}ms), ";
        summary += $"Slowest scenario: {slowest.Key} ({slowest.Value.Average():F3}ms). ";
    }
    
    return summary;
}
