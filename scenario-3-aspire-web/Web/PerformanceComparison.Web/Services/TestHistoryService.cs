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

        // De-duplication: if latest entry matches the new one, skip adding
        if (history.Count > 0 && IsDuplicate(history[0], item))
        {
            return;
        }

        // Add new item at the beginning
        history.Insert(0, item);

        // Trim to max items
        if (history.Count > MaxHistoryItems)
        {
            history = history.Take(MaxHistoryItems).ToList();
        }

        await SaveHistoryAsync(history);
    }

    private static bool IsDuplicate(TestHistoryItem a, TestHistoryItem b)
    {
        // Consider entries duplicate if key properties match within tolerances
        bool sameIterations = a.Iterations == b.Iterations;
        bool sameMode = string.Equals(a.TestMode, b.TestMode, StringComparison.OrdinalIgnoreCase);
        bool sameModel = string.Equals(a.Model ?? string.Empty, b.Model ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        bool close(double x, double y, double eps = 0.01) => Math.Abs(x - y) <= eps;

        bool sameTimes = close(a.DotNetAvgTime, b.DotNetAvgTime) &&
                         close(a.PythonAvgTime, b.PythonAvgTime) &&
                         close(a.DotNetTotalTime, b.DotNetTotalTime) &&
                         close(a.PythonTotalTime, b.PythonTotalTime);

        // If timestamp difference is within 5 minutes and all values match, treat as duplicate
        bool closeTime = Math.Abs((a.Timestamp - b.Timestamp).TotalMinutes) <= 5;

        return sameIterations && sameMode && sameModel && sameTimes && closeTime;
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
    public string? Model { get; set; }

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
