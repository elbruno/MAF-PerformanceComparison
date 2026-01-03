# Performance Comparison Report

Generated: 2026-01-03 17:33:55

## Overview

This report contains performance comparison data from 3 test runs.

## Test Results

### Comparison: HelloWorld - standard mode

#### Files Analyzed
- metrics_dotnet_helloworld_standard_20260103_173345.json
- metrics_python_helloworld_standard_20260103_173345.json

#### LLM Comparison Prompt

```
I have two performance test results from running the Microsoft Agent Framework in different environments. Please analyze and compare these results.

**First Test Result:**
```json
{
  "TestInfo": {
    "Language": "CSharp",
    "Framework": "DotNet",
    "Provider": "HelloWorld",
    "Model": "N/A (Demo Mode)",
    "Endpoint": "N/A (Demo Mode)",
    "TestMode": "standard",
    "Timestamp": "2026-01-03T17:33:45.1923071+00:00",
    "WarmupSuccessful": false
  },
  "Configuration": {
    "BatchSize": 10,
    "ConcurrentRequests": 5
  },
  "Metrics": {
    "TotalIterations": 10,
    "TotalExecutionTimeMs": 19,
    "AverageTimePerIterationMs": 0.0016259000000000002,
    "MinIterationTimeMs": 7.8e-05,
    "MaxIterationTimeMs": 0.013166,
    "MedianIterationTimeMs": 9.65e-05,
    "StandardDeviationMs": 0.0038768501763674075,
    "MemoryUsedMB": 0.007843017578125,
    "AverageCpuUsagePercent": 0,
    "TimeToFirstTokenMs": null,
    "ScenarioResults": null
  },
  "Summary": "Test completed in standard mode. Processed 10 iterations with average latency of 0.002ms. Memory usage: 0.01MB, CPU usage: 0.00%. ",
  "_filename": "metrics_dotnet_helloworld_standard_20260103_173345.json",
  "_filepath": "tests_results/20260103_173355_standard/metrics_dotnet_helloworld_standard_20260103_173345.json"
}
```

**Second Test Result:**
```json
{
  "TestInfo": {
    "Language": "Python",
    "Framework": "Python",
    "Provider": "HelloWorld",
    "Model": "N/A (Demo Mode)",
    "Endpoint": "N/A (Demo Mode)",
    "TestMode": "standard",
    "Timestamp": "2026-01-03T17:33:45.358250+00:00",
    "WarmupSuccessful": false
  },
  "Configuration": {
    "BatchSize": 10,
    "ConcurrentRequests": 5
  },
  "Metrics": {
    "TotalIterations": 10,
    "TotalExecutionTimeMs": 0.5626678466796875,
    "AverageTimePerIterationMs": 0.00040531158447265625,
    "MinIterationTimeMs": 0.0,
    "MaxIterationTimeMs": 0.0011920928955078125,
    "MedianIterationTimeMs": 0.0002384185791015625,
    "StandardDeviationMs": 0.0003381100874929849,
    "MemoryUsedMB": 0.18359375,
    "AverageCpuUsagePercent": 0,
    "TimeToFirstTokenMs": null,
    "ScenarioResults": null
  },
  "Summary": "Test completed in standard mode. Processed 10 iterations with average latency of 0.000ms. Memory usage: 0.18MB, CPU usage: 0.00%. ",
  "_filename": "metrics_python_helloworld_standard_20260103_173345.json",
  "_filepath": "tests_results/20260103_173355_standard/metrics_python_helloworld_standard_20260103_173345.json"
}
```

Please provide a comprehensive comparison that includes:

1. **Test Configuration Comparison:**
   - Language/Framework used (C#/.NET vs Python)
   - AI Provider and Model
   - Test mode and configuration
   - Warmup status
   - Test timestamp

2. **Performance Metrics Analysis:**
   - Total execution time comparison (which is faster and by what percentage?)
   - Average time per iteration (which has better average performance?)
   - Min/Max iteration times (which has more consistent performance?)
   - Performance variability (compare min/max ranges)
   - Standard deviation analysis

3. **Resource Usage:**
   - Memory consumption comparison
   - CPU utilization comparison
   - Efficiency analysis (performance vs resource usage)

4. **Test Mode Specific Metrics:**
   - Batch processing efficiency (if applicable)
   - Concurrent request handling (if applicable)
   - Streaming performance and TTFT (if applicable)
   - Scenario-specific results (if applicable)

5. **Key Insights:**
   - Which implementation performs better overall?
   - What are the notable differences?
   - Any recommendations based on the results?

6. **Statistical Summary:**
   - Provide a clear winner or note if results are comparable
   - Highlight any significant performance gaps
   - Consider both speed and resource efficiency

Please format your response in a clear, structured way with percentages and concrete numbers for easy understanding.
```

---

## Individual Test Summaries

### CSharp - HelloWorld - standard

**File:** `metrics_dotnet_helloworld_standard_20260103_173244.json`

**Test Information:**
- Language/Framework: CSharp / DotNet
- Provider: HelloWorld
- Model: N/A (Demo Mode)
- Test Mode: standard
- Timestamp: 2026-01-03T17:32:44.4990634+00:00
- Warmup Successful: False

**Performance Metrics:**
- Total Iterations: 10
- Total Execution Time: 19 ms
- Average Time per Iteration: 0.001507 ms
- Min Iteration Time: 7.2e-05 ms
- Max Iteration Time: 0.012142 ms
- Median Iteration Time: 9.7e-05 ms
- Standard Deviation: 0.0035877445282516984 ms
- Memory Used: 0.007843017578125 MB
- Average CPU Usage: 0%

**Summary:** Test completed in standard mode. Processed 10 iterations with average latency of 0.002ms. Memory usage: 0.01MB, CPU usage: 0.00%. 

---

### CSharp - HelloWorld - standard

**File:** `metrics_dotnet_helloworld_standard_20260103_173345.json`

**Test Information:**
- Language/Framework: CSharp / DotNet
- Provider: HelloWorld
- Model: N/A (Demo Mode)
- Test Mode: standard
- Timestamp: 2026-01-03T17:33:45.1923071+00:00
- Warmup Successful: False

**Performance Metrics:**
- Total Iterations: 10
- Total Execution Time: 19 ms
- Average Time per Iteration: 0.0016259000000000002 ms
- Min Iteration Time: 7.8e-05 ms
- Max Iteration Time: 0.013166 ms
- Median Iteration Time: 9.65e-05 ms
- Standard Deviation: 0.0038768501763674075 ms
- Memory Used: 0.007843017578125 MB
- Average CPU Usage: 0%

**Summary:** Test completed in standard mode. Processed 10 iterations with average latency of 0.002ms. Memory usage: 0.01MB, CPU usage: 0.00%. 

---

### Python - HelloWorld - standard

**File:** `metrics_python_helloworld_standard_20260103_173345.json`

**Test Information:**
- Language/Framework: Python / Python
- Provider: HelloWorld
- Model: N/A (Demo Mode)
- Test Mode: standard
- Timestamp: 2026-01-03T17:33:45.358250+00:00
- Warmup Successful: False

**Performance Metrics:**
- Total Iterations: 10
- Total Execution Time: 0.5626678466796875 ms
- Average Time per Iteration: 0.00040531158447265625 ms
- Min Iteration Time: 0.0 ms
- Max Iteration Time: 0.0011920928955078125 ms
- Median Iteration Time: 0.0002384185791015625 ms
- Standard Deviation: 0.0003381100874929849 ms
- Memory Used: 0.18359375 MB
- Average CPU Usage: 0%

**Summary:** Test completed in standard mode. Processed 10 iterations with average latency of 0.000ms. Memory usage: 0.18MB, CPU usage: 0.00%. 

---
