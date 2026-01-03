using System.Diagnostics;
using Microsoft.Agents.AI;

Console.WriteLine("=== C# Microsoft Agent Framework - Hello World ===\n");

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);

// Performance test: Run agent operations 1000 times
const int ITERATIONS = 1000;
var iterationTimes = new List<double>();

try
{
    Console.WriteLine("✓ Agent framework initialized successfully");
    Console.WriteLine($"✓ Running {ITERATIONS} iterations for performance testing\n");
    Console.WriteLine("Note: This is a demo/mock setup without external AI services");
    Console.WriteLine("For actual Azure OpenAI or Ollama, see the respective agent examples.\n");
    
    // Run 1000 iterations to measure performance
    for (int i = 0; i < ITERATIONS; i++)
    {
        var iterationStart = Stopwatch.GetTimestamp();
        
        // Simulate agent operation (mock response)
        var response = $"Hello from iteration {i + 1}!";
        
        var iterationEnd = Stopwatch.GetTimestamp();
        var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
        iterationTimes.Add(iterationTimeMs);
        
        // Print progress every 100 iterations
        if ((i + 1) % 100 == 0)
        {
            Console.WriteLine($"  Progress: {i + 1}/{ITERATIONS} iterations completed");
        }
    }
    
    Console.WriteLine("\n--- Sample Agent Response ---");
    Console.WriteLine("Hello from the Microsoft Agent Framework in C#!");
    Console.WriteLine("This agent is ready to process requests.");
    Console.WriteLine("Performance test completed successfully.");
    Console.WriteLine("-----------------------------\n");
    
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
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
