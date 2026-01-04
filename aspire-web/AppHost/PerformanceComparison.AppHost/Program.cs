var builder = DistributedApplication.CreateBuilder(args);

// Add .NET backend for running .NET agent tests
var dotnetBackend = builder.AddProject("dotnet-backend", "../../DotNetBackend/PerformanceComparison.DotNetBackend/PerformanceComparison.DotNetBackend.csproj")
    .WithHttpEndpoint(port: 5002, name: "http");

// Add Blazor frontend web app
var web = builder.AddProject("web", "../../Web/PerformanceComparison.Web/PerformanceComparison.Web.csproj")
    .WithExternalHttpEndpoints()
    .WithEnvironment("DotNetBackend", "http://localhost:5002")
    .WithEnvironment("PythonBackend", "http://localhost:5001");

builder.Build().Run();
