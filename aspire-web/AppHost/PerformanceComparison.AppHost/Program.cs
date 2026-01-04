var builder = DistributedApplication.CreateBuilder(args);

// add Python backend for running Python agent tests
// Use Aspire's Uvicorn integration for ASGI applications (FastAPI).
// See: https://aspire.dev/whats-new/aspire-13/#uvicorn-integration-for-asgi-applications
//
// The Python backend in this repository exposes a FastAPI app in
// `aspire-web/PythonBackend/main.py` with the FastAPI application instance
// named `app`. AddUvicornApp will run `uvicorn main:app` inside a virtual
// environment and will also manage package installation. We explicitly opt
// to use pip (requirements.txt) so Aspire will create or reuse a `.venv`
// inside the Python app directory and run `pip install -r requirements.txt`.
// If you prefer `uv` (pyproject.toml) change `.WithPip()` to `.WithUv()`.

var pythonBackend = builder.AddUvicornApp("app", "../../PythonBackend", "main:app")
    // Use pip to install dependencies from requirements.txt (auto-detects and creates .venv)
    .WithPip()
    // Ensure Aspire creates/uses the virtual environment named `.venv` in the app directory.
    .WithVirtualEnvironment(".venv", createIfNotExists: true)
    // Expose external HTTP endpoints. Do NOT call WithHttpEndpoint here as
    // WithExternalHttpEndpoints will already add an 'http' endpoint — calling
    // both can create a duplicate endpoint name conflict.
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
