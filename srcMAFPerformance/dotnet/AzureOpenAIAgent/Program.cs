using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

Console.WriteLine("=== C# Microsoft Agent Framework - Azure OpenAI Agent ===\n");

// Configuration - Read from environment variables or configuration
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4";

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);

try
{
    if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
    {
        Console.WriteLine("⚠ Configuration not found. Using demo mode.");
        Console.WriteLine("To use Azure OpenAI, set the following environment variables:");
        Console.WriteLine("  - AZURE_OPENAI_ENDPOINT");
        Console.WriteLine("  - AZURE_OPENAI_API_KEY");
        Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, defaults to 'gpt-4')");
        Console.WriteLine("\n✓ Kernel structure ready (demo mode)");
        Console.WriteLine("✓ Agent framework initialized");
    }
    else
    {
        // Create a kernel with Azure OpenAI chat completion service
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey);
        
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        Console.WriteLine("✓ Kernel initialized successfully");
        Console.WriteLine("✓ Azure OpenAI service connected");
        Console.WriteLine($"✓ Using deployment: {deploymentName}");
        
        // Simple chat interaction
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello! Please respond with a brief greeting.");
        
        var response = await chatService.GetChatMessageContentAsync(chatHistory);
        
        Console.WriteLine("\n--- Agent Response ---");
        Console.WriteLine(response.Content);
        Console.WriteLine("----------------------\n");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
}

// Stop performance measurement
stopwatch.Stop();
var endMemory = GC.GetTotalMemory(false);
var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0; // Convert to MB

Console.WriteLine("=== Performance Metrics ===");
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Memory Used: {memoryUsed:F2} MB");
Console.WriteLine("========================\n");
