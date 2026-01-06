namespace PerformanceComparison.Web.Models;

public class PerformanceInsights
{
    public string WinnerFramework { get; set; } = string.Empty;
    public double PercentageFaster { get; set; }
    public string MainInsight { get; set; } = string.Empty;
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Anomalies { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

public class IterationDataPoint
{
    public int Iteration { get; set; }
    public double TimeMs { get; set; }
    public double MemoryMB { get; set; }
}

public class DistributionBucket
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StatisticalSummary
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double StandardDeviation { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
    public double CoefficientOfVariation { get; set; }
    public string StabilityScore { get; set; } = string.Empty;
}
