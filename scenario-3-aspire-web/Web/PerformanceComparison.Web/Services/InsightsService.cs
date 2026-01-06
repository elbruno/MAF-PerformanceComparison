using PerformanceComparison.Web.Models;
using System.Globalization;
using CsvHelper;

namespace PerformanceComparison.Web.Services;

public class InsightsService
{
    public PerformanceInsights GenerateInsights(TestStatus? dotnet, TestStatus? python)
    {
        var insights = new PerformanceInsights();

        if (dotnet?.AverageTimePerIterationMs <= 0 || python?.AverageTimePerIterationMs <= 0)
            return insights;

        var dotnetAvg = dotnet!.AverageTimePerIterationMs;
        var pythonAvg = python!.AverageTimePerIterationMs;

        // Determine winner
        if (dotnetAvg < pythonAvg)
        {
            insights.WinnerFramework = ".NET";
            var diff = pythonAvg - dotnetAvg;
            insights.PercentageFaster = (diff / pythonAvg) * 100;
        }
        else
        {
            insights.WinnerFramework = "Python";
            var diff = dotnetAvg - pythonAvg;
            insights.PercentageFaster = (diff / dotnetAvg) * 100;
        }

        // Main insight
        insights.MainInsight = $"{insights.WinnerFramework} is {insights.PercentageFaster:F1}% faster on average ({Math.Abs(dotnetAvg - pythonAvg):F3}ms difference).";

        // Key findings
        insights.KeyFindings = new();

        var dotnetConsistency = CalculateConsistency(dotnet);
        var pythonConsistency = CalculateConsistency(python);

        if (dotnetConsistency > pythonConsistency + 5)
            insights.KeyFindings.Add($"🎯 .NET shows better consistency ({dotnetConsistency:F1}% vs {pythonConsistency:F1}%)");
        else if (pythonConsistency > dotnetConsistency + 5)
            insights.KeyFindings.Add($"🎯 Python shows better consistency ({pythonConsistency:F1}% vs {dotnetConsistency:F1}%)");

        var dotnetMemoryRatio = dotnet.MemoryUsedMB / Math.Max(1, dotnet.CurrentIteration / 1000.0);
        var pythonMemoryRatio = python.MemoryUsedMB / Math.Max(1, python.CurrentIteration / 1000.0);

        if (dotnetMemoryRatio < pythonMemoryRatio)
            insights.KeyFindings.Add($"💾 .NET uses {((pythonMemoryRatio - dotnetMemoryRatio) / pythonMemoryRatio * 100):F0}% less memory per 1000 iterations");
        else
            insights.KeyFindings.Add($"💾 Python uses {((dotnetMemoryRatio - pythonMemoryRatio) / dotnetMemoryRatio * 100):F0}% less memory per 1000 iterations");

        // Anomalies
        insights.Anomalies = new();
        if (dotnet.MaxIterationTimeMs > dotnet.AverageTimePerIterationMs * 3)
            insights.Anomalies.Add($"⚠️ .NET: Max iteration ({dotnet.MaxIterationTimeMs:F1}ms) is 3x higher than average—possible GC pause or timeout");

        if (python.MaxIterationTimeMs > python.AverageTimePerIterationMs * 3)
            insights.Anomalies.Add($"⚠️ Python: Max iteration ({python.MaxIterationTimeMs:F1}ms) is 3x higher than average—possible GC pause or timeout");

        // Recommendation
        if (dotnetAvg < pythonAvg * 1.1)
            insights.Recommendation = "Both frameworks show similar performance. Choose based on team expertise and ecosystem fit.";
        else if (dotnetAvg < pythonAvg)
            insights.Recommendation = "Use .NET for latency-critical workloads where consistent, fast response times matter.";
        else
            insights.Recommendation = "Use Python if throughput is acceptable and development speed is prioritized.";

        return insights;
    }

    private double CalculateConsistency(TestStatus status)
    {
        if (status?.AverageTimePerIterationMs <= 0)
            return 0;

        // Simple consistency metric: (avg - min) / (max - min) * 100
        var range = status.MaxIterationTimeMs - status.MinIterationTimeMs;
        if (range <= 0) return 100;

        var deviation = status.AverageTimePerIterationMs - status.MinIterationTimeMs;
        return Math.Max(0, 100 - (deviation / range * 100));
    }

    public string ExportToCsv(TestConfiguration config, TestStatus? dotnet, TestStatus? python)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        csv.WriteField("Metric");
        csv.WriteField(".NET");
        csv.WriteField("Python");
        csv.NextRecord();

        // Configuration
        csv.WriteField("Configuration");
        csv.WriteField("");
        csv.WriteField("");
        csv.NextRecord();

        csv.WriteField("Iterations");
        csv.WriteField(config.Iterations);
        csv.WriteField(config.Iterations);
        csv.NextRecord();

        csv.WriteField("Model");
        csv.WriteField(config.Model);
        csv.WriteField(config.Model);
        csv.NextRecord();

        csv.WriteField("Endpoint");
        csv.WriteField(config.Endpoint);
        csv.WriteField(config.Endpoint);
        csv.NextRecord();

        csv.WriteField("");
        csv.NextRecord();

        // Results
        csv.WriteField("Results");
        csv.WriteField("");
        csv.WriteField("");
        csv.NextRecord();

        csv.WriteField("Status");
        csv.WriteField(dotnet?.Status ?? "N/A");
        csv.WriteField(python?.Status ?? "N/A");
        csv.NextRecord();

        csv.WriteField("Avg Time (ms)");
        csv.WriteField(dotnet?.AverageTimePerIterationMs ?? 0);
        csv.WriteField(python?.AverageTimePerIterationMs ?? 0);
        csv.NextRecord();

        csv.WriteField("Min Time (ms)");
        csv.WriteField(dotnet?.MinIterationTimeMs ?? 0);
        csv.WriteField(python?.MinIterationTimeMs ?? 0);
        csv.NextRecord();

        csv.WriteField("Max Time (ms)");
        csv.WriteField(dotnet?.MaxIterationTimeMs ?? 0);
        csv.WriteField(python?.MaxIterationTimeMs ?? 0);
        csv.NextRecord();

        csv.WriteField("Memory (MB)");
        csv.WriteField(dotnet?.MemoryUsedMB ?? 0);
        csv.WriteField(python?.MemoryUsedMB ?? 0);
        csv.NextRecord();

        csv.WriteField("Success Rate");
        var dotnetRate = dotnet?.TotalIterations > 0 ? (dotnet.SuccessCount / (double)dotnet.TotalIterations * 100) : 0;
        var pythonRate = python?.TotalIterations > 0 ? (python.SuccessCount / (double)python.TotalIterations * 100) : 0;
        csv.WriteField($"{dotnetRate:F1}%");
        csv.WriteField($"{pythonRate:F1}%");
        csv.NextRecord();

        csv.WriteField("Elapsed Time (s)");
        csv.WriteField(dotnet?.ElapsedTimeMs / 1000.0 ?? 0);
        csv.WriteField(python?.ElapsedTimeMs / 1000.0 ?? 0);
        csv.NextRecord();

        return writer.ToString();
    }
}
