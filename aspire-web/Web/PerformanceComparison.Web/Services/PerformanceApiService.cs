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
        _dotnetClient = httpClientFactory.CreateClient("dotnetBackend");
        _pythonClient = httpClientFactory.CreateClient("pythonBackend");
        _logger = logger;
    }

    // .NET Backend Methods
    public async Task<StartTestResponse?> StartDotNetTestAsync(TestConfiguration config)
    {
        try
        {
            var response = await _dotnetClient.PostAsJsonAsync("api/performance/start", config);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StartTestResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start .NET test");
            return null;
        }
    }

    public async Task<bool> StopDotNetTestAsync()
    {
        try
        {
            var response = await _dotnetClient.PostAsync("api/performance/stop", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop .NET test");
            return false;
        }
    }

    public async Task<TestStatus?> GetDotNetStatusAsync(string? sessionId = null)
    {
        try
        {
            var url = string.IsNullOrEmpty(sessionId) ? "api/performance/status" : $"api/performance/status?sessionId={sessionId}";
            var response = await _dotnetClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TestStatus>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get .NET test status");
            return null;
        }
    }

    // Python Backend Methods
    public async Task<StartTestResponse?> StartPythonTestAsync(TestConfiguration config)
    {
        try
        {
            var response = await _pythonClient.PostAsJsonAsync("api/performance/start", config);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<StartTestResponse>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Python test");
            return null;
        }
    }

    public async Task<bool> StopPythonTestAsync()
    {
        try
        {
            var response = await _pythonClient.PostAsync("api/performance/stop", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Python test");
            return false;
        }
    }

    public async Task<TestStatus?> GetPythonStatusAsync(string? sessionId = null)
    {
        try
        {
            var url = string.IsNullOrEmpty(sessionId) ? "api/performance/status" : $"api/performance/status?sessionId={sessionId}";
            var response = await _pythonClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TestStatus>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Python test status");
            return null;
        }
    }

    // Health Check Methods
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

public class StartTestResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class TestStatus
{
    public string? SessionId { get; set; }
    public string Status { get; set; } = "Idle";
    public int CurrentIteration { get; set; }
    public int TotalIterations { get; set; }
    public long ElapsedTimeMs { get; set; }
    public double ProgressPercentage { get; set; }
    public double AverageTimePerIterationMs { get; set; }
    public double MinIterationTimeMs { get; set; }
    public double MaxIterationTimeMs { get; set; }
    public double LastIterationTimeMs { get; set; }
    public double IterationsPerSecond { get; set; }
    public double EstimatedTimeRemainingMs { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double MemoryUsedMB { get; set; }
    public bool WarmupSuccessful { get; set; }
    public double WarmupTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public TestConfiguration? Configuration { get; set; }
    public Dictionary<string, object>? MachineInfo { get; set; }
}
