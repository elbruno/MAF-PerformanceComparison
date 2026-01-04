var builder = DistributedApplication.CreateBuilder(args);

// add Python backend for running Python agent tests
// var pythonBackend = builder.AddPythonApp("app", "../../PythonBackend", "main")
var pythonBackend = builder.AddPythonModule("app", "../../PythonBackend", "main")
    .WithHttpEndpoint(port: 5001, env: "PORT")
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
