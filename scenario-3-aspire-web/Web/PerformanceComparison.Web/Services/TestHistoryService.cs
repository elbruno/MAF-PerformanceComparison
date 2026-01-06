using Microsoft.JSInterop;
using PerformanceComparison.Web.Models;
using System.Text.Json;

namespace PerformanceComparison.Web.Services;

/// <summary>
/// Service for managing test history using browser local storage
/// </summary>
public class TestHistoryService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "performanceTestHistory";
    private const int MaxHistoryItems = 20; // Keep last 20 test results

    public TestHistoryService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Save a test result to history
    /// </summary>
    public async Task SaveTestResultAsync(TestHistoryItem item)
    {
        var history = await GetHistoryAsync();
        
        // Add new item at the beginning
        history.Insert(0, item);
        
        // Trim to max items
        if (history.Count > MaxHistoryItems)
        {
            history = history.Take(MaxHistoryItems).ToList();
        }
        
        await SaveHistoryAsync(history);
    }

    /// <summary>
    /// Get all test history items
    /// </summary>
    public async Task<List<TestHistoryItem>> GetHistoryAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            
            if (string.IsNullOrEmpty(json))
                return new List<TestHistoryItem>();
            
            return JsonSerializer.Deserialize<List<TestHistoryItem>>(json) ?? new List<TestHistoryItem>();
        }
        catch
        {
            return new List<TestHistoryItem>();
        }
    }

    /// <summary>
    /// Clear all test history
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

    /// <summary>
    /// Delete a specific test result by ID
    /// </summary>
    public async Task DeleteTestResultAsync(string id)
    {
        var history = await GetHistoryAsync();
        history.RemoveAll(h => h.Id == id);
        await SaveHistoryAsync(history);
    }

    private async Task SaveHistoryAsync(List<TestHistoryItem> history)
    {
        var json = JsonSerializer.Serialize(history);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}

/// <summary>
/// Represents a saved test result in history
/// </summary>
public class TestHistoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TestMode { get; set; } = "Standard";
    public int Iterations { get; set; }
    
    // .NET Results
    public double DotNetAvgTime { get; set; }
    public double DotNetMinTime { get; set; }
    public double DotNetMaxTime { get; set; }
    public double DotNetMemory { get; set; }
    public double DotNetTotalTime { get; set; }
    
    // Python Results
    public double PythonAvgTime { get; set; }
    public double PythonMinTime { get; set; }
    public double PythonMaxTime { get; set; }
    public double PythonMemory { get; set; }
    public double PythonTotalTime { get; set; }
    
    // Summary
    public string Winner { get; set; } = "";
    public double PercentageDifference { get; set; }
}
