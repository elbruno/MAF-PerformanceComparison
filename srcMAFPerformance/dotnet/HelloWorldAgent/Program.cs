using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

Console.WriteLine("=== C# Microsoft Agent Framework - Hello World ===\n");

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);

try
{
    // Create a kernel with chat completion service
    // Note: This is a mock/demo setup. For actual Azure OpenAI or Ollama, 
    // you would need to configure with actual endpoints and keys
    var builder = Kernel.CreateBuilder();
    
    // For demo purposes, we're showing the structure
    // In production, you would use:
    // builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
    // or
    // builder.AddOpenAIChatCompletion(modelId, apiKey); // for Ollama with OpenAI-compatible API
    
    var kernel = builder.Build();
    
    Console.WriteLine("✓ Kernel initialized successfully");
    Console.WriteLine("✓ Agent framework ready");
    Console.WriteLine("\n--- Hello World Agent Response ---");
    Console.WriteLine("Hello from the Microsoft Agent Framework in C#!");
    Console.WriteLine("This agent is ready to process requests.");
    Console.WriteLine("-----------------------------------\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Stop performance measurement
stopwatch.Stop();
var endMemory = GC.GetTotalMemory(false);
var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0; // Convert to MB

Console.WriteLine("=== Performance Metrics ===");
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Memory Used: {memoryUsed:F2} MB");
Console.WriteLine("========================\n");
