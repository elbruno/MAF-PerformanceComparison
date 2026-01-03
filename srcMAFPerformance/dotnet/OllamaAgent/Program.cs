using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

Console.WriteLine("=== C# Microsoft Agent Framework - Ollama Agent ===\n");

// Configuration - Read from environment variables or use defaults
var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
var modelId = Environment.GetEnvironmentVariable("OLLAMA_MODEL_ID") ?? "llama2";

// Start performance measurement
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);

try
{
    Console.WriteLine($"Configuring for Ollama endpoint: {endpoint}");
    Console.WriteLine($"Using model: {modelId}");
    Console.WriteLine("\nNote: This requires Ollama to be running locally.");
    Console.WriteLine("Install Ollama from: https://ollama.ai/");
    Console.WriteLine($"Start Ollama and pull the model: ollama pull {modelId}\n");
    
    // Validate endpoint URL
    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
    {
        throw new ArgumentException($"Invalid endpoint URL: {endpoint}");
    }
    
    // Create a kernel with OpenAI-compatible chat completion service for Ollama
    var builder = Kernel.CreateBuilder();
    
    // Ollama provides an OpenAI-compatible API
    builder.AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: null, // Ollama doesn't require an API key
        endpoint: endpointUri);
    
    var kernel = builder.Build();
    
    Console.WriteLine("✓ Kernel initialized successfully");
    Console.WriteLine("✓ Ollama service configured");
    
    try
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        // Simple chat interaction
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello! Please respond with a brief greeting in 10 words or less.");
        
        Console.WriteLine("\n--- Attempting to connect to Ollama ---");
        var response = await chatService.GetChatMessageContentAsync(chatHistory);
        
        Console.WriteLine("\n--- Agent Response ---");
        Console.WriteLine(response.Content);
        Console.WriteLine("----------------------\n");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"\n⚠ Could not connect to Ollama at {endpoint}");
        Console.WriteLine("Please ensure Ollama is running and the model is available.");
        Console.WriteLine($"Error: {ex.Message}");
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
