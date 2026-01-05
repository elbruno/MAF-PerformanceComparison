var builder = DistributedApplication.CreateBuilder(args);

// See: https://aspire.dev/whats-new/aspire-13/#uvicorn-integration-for-asgi-applications
var pythonBackend = builder.AddUvicornApp("pythonBackend", "../../PythonBackend", "main:app")
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
    //// Ensure the Blazor web project binds to the same URLs used in its launchSettings
    //// so you can navigate to https://localhost:7274/dashboard when running locally.
    //.WithEnvironment("ASPNETCORE_URLS", "https://localhost:7274;http://localhost:5243")
    .WithEnvironment("PythonBackend", pythonBackEndpoint);

builder.Build().Run();
