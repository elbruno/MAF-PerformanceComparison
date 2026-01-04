var builder = DistributedApplication.CreateBuilder(args);

// Add Python backend for running Python agent tests
var pythonBackend = builder.AddPythonProject("python-backend", 
    "../../../python/ollama_agent", 
    "main.py")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithEnvironment("PYTHONUNBUFFERED", "1");

// Add .NET backend for running .NET agent tests
var dotnetBackend = builder.AddProject<Projects.PerformanceComparison_DotNetBackend>("dotnet-backend")
    .WithHttpEndpoint(port: 5002, name: "http");

// Add Blazor frontend web app
var web = builder.AddProject<Projects.PerformanceComparison_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(pythonBackend)
    .WithReference(dotnetBackend);

builder.Build().Run();
