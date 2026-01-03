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

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);

// Performance test: Run agent operations. Make configurable via environment variable for easier testing.
var ITERATIONS = int.TryParse(Environment.GetEnvironmentVariable("ITERATIONS"), out var iters) ? iters : 1000;
var iterationTimes = new List<double>();
var warmupSuccessful = false;

try
{
    if (string.IsNullOrEmpty(endpoint))
    {
        Console.WriteLine("⚠ AZURE_OPENAI_ENDPOINT not found. Using demo mode.");
        Console.WriteLine("To use Azure OpenAI, set the following environment variables:");
        Console.WriteLine("  - AZURE_OPENAI_ENDPOINT");
        Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, defaults to 'gpt-5-mini')");
        Console.WriteLine($"\n✓ Agent framework structure ready (demo mode)");
        Console.WriteLine($"✓ Running {ITERATIONS} iterations for performance testing\n");

        // Run 1000 iterations in demo mode (mock responses)
        for (int i = 0; i < ITERATIONS; i++)
        {
            var iterationStart = Stopwatch.GetTimestamp();

            // Simulate agent operation (mock response)
            var response = $"Hello from iteration {i + 1}! (Demo mode)";

            var iterationEnd = Stopwatch.GetTimestamp();
            var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
            iterationTimes.Add(iterationTimeMs);

            if ((i + 1) % 100 == 0)
            {
                Console.WriteLine($"  Progress: {i + 1}/{ITERATIONS} iterations completed");
            }
        }
    }
    else
    {
        // Create agent using Microsoft.Agents.AI with Azure OpenAI
        AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new AzureCliCredential())
            .GetChatClient(deploymentName)
            .CreateAIAgent(instructions: "You are a helpful assistant. Provide brief, concise responses.", name: "PerformanceTestAgent");

        Console.WriteLine("✓ Agent framework initialized successfully");
        Console.WriteLine("✓ Azure OpenAI service configured");
        Console.WriteLine($"✓ Using deployment: {deploymentName}");
        
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

        // Run 1000 iterations with actual API calls
        for (int i = 0; i < ITERATIONS; i++)
        {
            var iterationStart = Stopwatch.GetTimestamp();

            // Invoke the agent
            var response = await agent.RunAsync($"Say hello {i + 1}");

            var iterationEnd = Stopwatch.GetTimestamp();
            var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
            iterationTimes.Add(iterationTimeMs);

            if ((i + 1) % 100 == 0)
            {
                Console.WriteLine($"  Progress: {i + 1}/{ITERATIONS} iterations completed");
            }
        }

        // Show a sample streaming response
        Console.WriteLine("\n--- Sample Agent Streaming Response ---");
        await foreach (var update in agent.RunStreamingAsync("Tell me a brief joke about a pirate."))
        {
            Console.Write(update);
        }
        Console.WriteLine("\n---------------------------\n");
    }

    // Calculate statistics
    var avgIterationTime = iterationTimes.Average();
    var minIterationTime = iterationTimes.Min();
    var maxIterationTime = iterationTimes.Max();

    // Stop performance measurement
    stopwatch.Stop();
    var endMemory = GC.GetTotalMemory(false);
    var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0; // Convert to MB

    Console.WriteLine("=== Performance Metrics ===");
    Console.WriteLine($"Total Iterations: {ITERATIONS}");
    Console.WriteLine($"Total Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine($"Average Time per Iteration: {avgIterationTime:F3} ms");
    Console.WriteLine($"Min Iteration Time: {minIterationTime:F3} ms");
    Console.WriteLine($"Max Iteration Time: {maxIterationTime:F3} ms");
    Console.WriteLine($"Memory Used: {memoryUsed:F2} MB");
    Console.WriteLine("========================\n");
    
    // Export metrics to JSON file
    var currentTimestamp = DateTimeOffset.UtcNow;
    var metricsData = new
    {
        TestInfo = new
        {
            Language = "CSharp",
            Framework = "DotNet",
            Provider = "AzureOpenAI",
            Model = deploymentName,
            Endpoint = endpoint ?? "N/A (Demo Mode)",
            Timestamp = currentTimestamp.ToString("o"),
            WarmupSuccessful = warmupSuccessful
        },
        Metrics = new
        {
            TotalIterations = ITERATIONS,
            TotalExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            AverageTimePerIterationMs = avgIterationTime,
            MinIterationTimeMs = minIterationTime,
            MaxIterationTimeMs = maxIterationTime,
            MemoryUsedMB = memoryUsed
        }
    };

    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var jsonContent = JsonSerializer.Serialize(metricsData, jsonOptions);
    var timestamp = currentTimestamp.ToString("yyyyMMdd_HHmmss");
    var outputFileName = $"metrics_dotnet_azureopenai_{timestamp}.json";
    await File.WriteAllTextAsync(outputFileName, jsonContent);
    Console.WriteLine($"✓ Metrics exported to: {outputFileName}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine(ex.StackTrace);
}
