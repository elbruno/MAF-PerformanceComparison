var builder = DistributedApplication.CreateBuilder(args);

// add Python backend for running Python agent tests
var pythonBackend = builder.AddUvicornApp("app", "./app", "main:app")
    .WithUv()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");
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
