using PerformanceComparison.Web.Components;
using PerformanceComparison.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP clients for backend services
//builder.Services.AddHttpClient("dotnet-backend", client =>
//{
//    var dotnetBackendUrl = builder.Configuration["services:dotnet-backend:http:0"] 
//        ?? builder.Configuration["DotNetBackend"] 
//        ?? "http://localhost:5002";
//    client.BaseAddress = new Uri(dotnetBackendUrl);
//});

//builder.Services.AddHttpClient("python-backend", client =>
//{
//    var pythonBackendUrl = builder.Configuration["services:python-backend:http:0"]
//        ?? builder.Configuration["PythonBackend"]
//        ?? "http://localhost:5001";
//    client.BaseAddress = new Uri(pythonBackendUrl);
//});

builder.Services.AddHttpClient("dotnetBackend", client =>
{
    client.BaseAddress = new("https+http://dotnetBackend");
});
builder.Services.AddHttpClient("pythonBackend", client =>
{
    client.BaseAddress = new("https+http://pythonBackend");
});

builder.Services.AddScoped<PerformanceApiService>();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
