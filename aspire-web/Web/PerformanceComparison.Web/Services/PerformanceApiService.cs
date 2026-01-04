using System.Net.Http.Json;
using System.Text.Json;
using PerformanceComparison.Web.Models;

namespace PerformanceComparison.Web.Services;

public class PerformanceApiService
{
    private readonly HttpClient _dotnetClient;
    private readonly HttpClient _pythonClient;
    private readonly ILogger<PerformanceApiService> _logger;

    public PerformanceApiService(
        IHttpClientFactory httpClientFactory,
        ILogger<PerformanceApiService> logger)
    {
        _dotnetClient = httpClientFactory.CreateClient("dotnet-backend");
        _pythonClient = httpClientFactory.CreateClient("python-backend");
        _logger = logger;
    }

    public async Task<TestResult?> RunDotNetTestAsync(TestConfiguration config)
    {
        try
        {
            var response = await _dotnetClient.PostAsJsonAsync("api/performance/run", config);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TestResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run .NET test");
            return new TestResult 
            { 
                Success = false, 
                Message = $"Failed to run .NET test: {ex.Message}" 
            };
        }
    }

    public async Task<TestResult?> RunPythonTestAsync(TestConfiguration config)
    {
        try
        {
            var response = await _pythonClient.PostAsJsonAsync("api/performance/run", config);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            return JsonSerializer.Deserialize<TestResult>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run Python test");
            return new TestResult 
            { 
                Success = false, 
                Message = $"Failed to run Python test: {ex.Message}" 
            };
        }
    }

    public async Task<bool> CheckDotNetHealthAsync()
    {
        try
        {
            var response = await _dotnetClient.GetAsync("api/performance/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckPythonHealthAsync()
    {
        try
        {
            var response = await _pythonClient.GetAsync("api/performance/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
