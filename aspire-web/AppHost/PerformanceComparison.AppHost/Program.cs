var builder = DistributedApplication.CreateBuilder(args);

// See: https://aspire.dev/whats-new/aspire-13/#uvicorn-integration-for-asgi-applications
var pythonBackend = builder.AddUvicornApp("app", "../../PythonBackend", "main:app")
    .WithPip()
    .WithVirtualEnvironment(".venv", createIfNotExists: true)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/api/performance/health");
var pythonBackEndpoint = pythonBackend.GetEndpoint("http");

// Add .NET backend for running .NET agent tests
var dotnetBackend = builder.AddProject<Projects.PerformanceComparison_DotNetBackend>("dotnetBackend")
    .WithExternalHttpEndpoints();

// Add Blazor frontend web app
var web = builder.AddProject<Projects.PerformanceComparison_Web>("web")
    .WaitFor(pythonBackend)
    .WithReference(pythonBackend)
    .WaitFor(dotnetBackend)
    .WithReference(dotnetBackend)
    .WithExternalHttpEndpoints()
    .WithEnvironment("PythonBackend", pythonBackEndpoint);

builder.Build().Run();
