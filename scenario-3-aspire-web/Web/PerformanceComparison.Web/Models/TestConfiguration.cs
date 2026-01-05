namespace PerformanceComparison.Web.Models;

public class TestConfiguration
{
    public int Iterations { get; set; } = 10;
    public string Model { get; set; } = "ministral-3";
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string TestMode { get; set; } = "standard";
    public int BatchSize { get; set; } = 10;
    public int ConcurrentRequests { get; set; } = 5;
}

public class TestMetrics
{
    public string Language { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool WarmupSuccessful { get; set; }
    public int TotalIterations { get; set; }
    public double TotalExecutionTimeMs { get; set; }
    public double AverageTimePerIterationMs { get; set; }
    public double MinIterationTimeMs { get; set; }
    public double MaxIterationTimeMs { get; set; }
    public double MemoryUsedMB { get; set; }
    public Dictionary<string, object> MachineInfo { get; set; } = new();
}

public class TestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TestMetrics? Metrics { get; set; }
}
