using PerformanceComparison.Web.Models;
using System.Globalization;
using CsvHelper;

namespace PerformanceComparison.Web.Services;

public class InsightsService
{
    /// <summary>
    /// Sanitizes string values to prevent CSV injection attacks by prefixing potentially dangerous characters.
    /// Protects against formula injection in Excel, Google Sheets, and other spreadsheet applications.
    /// </summary>
    private static string SanitizeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        // Trim whitespace to check actual content
        var trimmed = value.TrimStart();
        if (trimmed.Length == 0)
            return value;

        // Check if the value starts with potentially dangerous characters
        // =, +, -, @ can start formulas; \t, \r, | can be used in injection attacks
        char firstChar = trimmed[0];
        if (firstChar == '=' || firstChar == '+' || firstChar == '-' || firstChar == '@' || 
            firstChar == '\t' || firstChar == '\r' || firstChar == '|')
        {
            // Prefix with single quote to prevent formula interpretation
            return "'" + value;
        }

        return value;
    }

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

        // Calculate memory usage per 1000 iterations
        // Ensure we have at least 1000 iterations to avoid inflated ratios
        var dotnetIterations = Math.Max(1000, dotnet.CurrentIteration);
        var pythonIterations = Math.Max(1000, python.CurrentIteration);

        var dotnetMemoryRatio = dotnet.MemoryUsedMB / (dotnetIterations / 1000.0);
        var pythonMemoryRatio = python.MemoryUsedMB / (pythonIterations / 1000.0);

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

    public double CalculateConsistency(TestStatus status)
    {
        if (status?.AverageTimePerIterationMs <= 0)
            return 0;

        // Consistency metric based on dispersion: smaller range relative to average => higher consistency
        var range = status.MaxIterationTimeMs - status.MinIterationTimeMs;
        if (range <= 0) return 100;

        var avg = status.AverageTimePerIterationMs;
        if (avg <= 0) return 0;

        var variability = range / avg; // dimensionless measure of spread relative to mean
        var score = 100 - (variability * 100);

        // Clamp score to [0, 100]
        return Math.Max(0, Math.Min(100, score));
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
        csv.WriteField(SanitizeCsvField(config.Model));
        csv.WriteField(SanitizeCsvField(config.Model));
        csv.NextRecord();

        csv.WriteField("Endpoint");
        csv.WriteField(SanitizeCsvField(config.Endpoint));
        csv.WriteField(SanitizeCsvField(config.Endpoint));
        csv.NextRecord();

        csv.WriteField("");
        csv.NextRecord();

        // Results
        csv.WriteField("Results");
        csv.WriteField("");
        csv.WriteField("");
        csv.NextRecord();

        csv.WriteField("Status");
        csv.WriteField(SanitizeCsvField(dotnet?.Status) ?? "N/A");
        csv.WriteField(SanitizeCsvField(python?.Status) ?? "N/A");
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
