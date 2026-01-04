using PerformanceComparison.DotNetBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add performance test service
builder.Services.AddSingleton<PerformanceTestService>();

var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

app.Run();
