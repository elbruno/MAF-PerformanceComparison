# Performance Comparison Report

Generated: 2026-01-03 20:02:15

This report summarizes Ollama performance test results and provides prompts to generate detailed comparisons.

## Overview

Found 2 Ollama test run(s).

## Test Results

### Comparison: Ollama - standard

#### Files Analyzed
- metrics_dotnet_ollama_20260104_010023.json
- metrics_python_ollama_20260104_010214.json

#### LLM Comparison Prompt

```
I have two performance test results from running the Microsoft Agent Framework in different environments. Please analyze and compare these results.

**First Test Result:**
```json
{
  "TestInfo": {
    "Language": "CSharp",
    "Framework": "DotNet",
    "Provider": "Ollama",
    "Model": "ministral-3",
    "Endpoint": "http://localhost:11434",
    "Timestamp": "2026-01-04T01:00:23.4861121+00:00",
    "WarmupSuccessful": true
  },
  "MachineInfo": {
    "OSDescription": "Microsoft Windows 10.0.26220",
    "ProcessorCount": 32,
    "Architecture": "X64",
    "DotNetVersion": ".NET 10.0.1",
    "TotalMemoryGB": 31.71,
    "CPUModel": "Intel(R) Core(TM) i9-14900KF",
    "CPUMaxSpeedGHz": 3.2,
    "AvailableMemoryGB": 0.01
  },
  "Metrics": {
    "TotalIterations": 500,
    "TotalExecutionTimeMs": 111145,
    "AverageTimePerIterationMs": 221.12662539999977,
    "MinIterationTimeMs": 129.5677,
    "MaxIterationTimeMs": 958.7416,
    "MemoryUsedMB": 8.740066528320312
  },
  "_filename": "metrics_dotnet_ollama_20260104_010023.json",
  "_filepath": "tests_results\\20260103_200215_standard_500iter\\metrics_dotnet_ollama_20260104_010023.json"
}
```

**Second Test Result:**
```json
{
  "TestInfo": {
    "Language": "Python",
    "Framework": "Python",
    "Provider": "Ollama",
    "Model": "ministral-3",
    "Endpoint": "http://localhost:11434",
    "Timestamp": "2026-01-04T01:02:14.685407+00:00",
    "WarmupSuccessful": true
  },
  "MachineInfo": {
    "OSSystem": "Windows",
    "OSRelease": "11",
    "OSVersion": "10.0.26220",
    "Architecture": "AMD64",
    "ProcessorCount": 24,
    "LogicalProcessorCount": 32,
    "CPUMaxFreqGHz": 3.2,
    "CPUCurrentFreqGHz": 3.2,
    "TotalMemoryGB": 31.71,
    "AvailableMemoryGB": 12.95,
    "MemoryPercentUsed": 59.1,
    "TotalSwapGB": 22.0,
    "GPUModel": "NVIDIA GeForce RTX 4070 Ti SUPER",
    "GPUMemoryMB": "16376 MiB",
    "PythonVersion": "3.14.2"
  },
  "Metrics": {
    "TotalIterations": 500,
    "TotalExecutionTimeMs": 109968.57929229736,
    "AverageTimePerIterationMs": 218.1537628173828,
    "MinIterationTimeMs": 133.5597038269043,
    "MaxIterationTimeMs": 441.27917289733887,
    "MemoryUsedMB": 4.87109375
  },
  "_filename": "metrics_python_ollama_20260104_010214.json",
  "_filepath": "tests_results\\20260103_200215_standard_500iter\\metrics_python_ollama_20260104_010214.json"
}
```

Please provide a comprehensive comparison that includes:

1. **Test Environment & Configuration:**
   - Language/Framework used (C#/.NET vs Python)
   - AI Provider and Model
   - Runtime versions (.NET / Python)
   - Machine specifications (OS, CPU, Memory, GPU if available)
   - Warmup status
   - Test timestamp

2. **Machine Hardware Comparison:**
   - Processor count and clock speed
   - Available system memory
   - GPU capabilities (if applicable)
   - Impact of hardware differences on performance

3. **Performance Metrics Analysis:**
   - Total execution time comparison (which is faster and by what percentage?)
   - Average time per iteration (which has better average performance?)
   - Min/Max iteration times (which has more consistent performance?)
   - Performance variability (compare min/max ranges)

4. **Resource Usage & Efficiency:**
   - Memory consumption comparison
   - CPU utilization metrics
   - Efficiency analysis (performance vs resource usage)
   - Scalability implications

5. **Key Insights:**
   - Which implementation performs better overall?
   - How do machine specifications influence performance?
   - What are the notable differences?
   - Any recommendations based on the results?

6. **Statistical Summary:**
   - Provide a clear winner or note if results are comparable
   - Highlight any significant performance gaps
   - Consider both speed and resource efficiency
   - Account for hardware differences in conclusions

Please format your response in a clear, structured way with percentages and concrete numbers for easy understanding.

```

## Usage Instructions

1. Run the performance tests for both implementations you want to compare:
   - For .NET: `cd dotnet/[AgentType] && dotnet run`
   - For Python: `cd python/[agent_type] && python main.py`

2. Locate the generated metrics JSON files:
   - .NET Ollama: `metrics_dotnet_ollama_[timestamp].json`
   - Python Ollama: `metrics_python_ollama_[timestamp].json`
   - .NET Azure OpenAI: `metrics_dotnet_azureopenai_[timestamp].json`
   - Python Azure OpenAI: `metrics_python_azureopenai_[timestamp].json`

3. Copy the content of both JSON files

4. Use the prompt template above, replacing:
   - `[PASTE CONTENT OF FIRST METRICS FILE HERE]` with the first file's content
   - `[PASTE CONTENT OF SECOND METRICS FILE HERE]` with the second file's content

5. Submit the prompt to your preferred LLM (ChatGPT, Claude, Copilot, etc.)

## Example Comparisons

### Common Comparison Scenarios:

1. **.NET vs Python (Same Provider & Model)**
   - Compare: `metrics_dotnet_ollama_*.json` vs `metrics_python_ollama_*.json`
   - Purpose: See which language implementation is more efficient

2. **Ollama vs Azure OpenAI (Same Language)**
   - Compare: `metrics_dotnet_ollama_*.json` vs `metrics_dotnet_azureopenai_*.json`
   - Purpose: Compare local vs cloud AI performance

3. **Cross-Platform Full Comparison**
   - Compare any two metrics files to understand the trade-offs

## Metrics File Structure

Each metrics JSON file now contains comprehensive information:

```json
{
  "TestInfo": {
    "Language": "CSharp" or "Python",
    "Framework": "DotNet" or "Python",
    "Provider": "Ollama" or "AzureOpenAI",
    "Model": "model-name",
    "Endpoint": "service-endpoint",
    "Timestamp": "ISO-8601 timestamp",
    "WarmupSuccessful": true/false
  },
  "MachineInfo": {
    "OSSystem": "Windows/Linux/macOS",
    "OSRelease": "OS version",
    "Architecture": "x86_64/ARM64",
    "ProcessorCount": 8,
    "LogicalProcessorCount": 16,
    "CPUMaxFreqGHz": 3.6,
      "DotNetVersion": ".NET 10.0.0",
    "TotalMemoryGB": 16.0,
    "AvailableMemoryGB": 8.5,
    "MemoryPercentUsed": 47.5,
    "GPUModel": "NVIDIA GeForce RTX 3080",
    "GPUMemoryMB": "10240",
    "PythonVersion": "3.11.0"
  },
  "Metrics": {
    "TotalIterations": 1000,
    "TotalExecutionTimeMs": number,
    "AverageTimePerIterationMs": number,
    "MinIterationTimeMs": number,
    "MaxIterationTimeMs": number,
    "MemoryUsedMB": number
  }
}
```

## Tips for Analysis

- **Consider machine specifications**: Hardware differences (CPU, RAM, GPU) significantly impact performance - normalize expectations based on hardware
- **Consider warmup status**: Tests with successful warmups may show different performance characteristics
- **Check timestamps**: Ensure tests were run under similar conditions (similar system load, network conditions, etc.)
- **Look at consistency**: Min/Max ranges indicate performance stability
- **Memory matters**: Lower memory usage is better for scalability
- **CPU comparison**: More cores = potentially better parallel performance
- **GPU impact**: GPU availability can dramatically affect certain workloads
- **Average vs Total**: Both metrics are important - average shows per-operation efficiency, total shows cumulative overhead
- **Hardware-normalized analysis**: When comparing different machines, consider performance-per-watt or performance-per-core metrics
```

---

## Individual Test Summaries

### CSharp - Ollama - standard

**File:** `metrics_dotnet_ollama_20260104_010023.json`

**Test Information:**
- Language/Framework: CSharp / DotNet
- Provider: Ollama
- Model: ministral-3
- Test Mode: standard
- Endpoint: http://localhost:11434
- Timestamp: 2026-01-04T01:00:23.4861121+00:00
- Warmup Successful: True

**Machine Information:**
- OS: N/A 
- Architecture: X64
- Processors: 32 cores (N/A logical)
- .NET Version: .NET 10.0.1
- CPU Max Speed: 3.2 GHz
- Total Memory: 31.71 GB
- Available Memory: 0.01 GB

**Performance Metrics:**
- Total Iterations: 500
- Total Execution Time: 111145 ms
- Average Time per Iteration: 221.12662539999977 ms
- Min Iteration Time: 129.5677 ms
- Max Iteration Time: 958.7416 ms
- Median Iteration Time: N/A
- Standard Deviation: N/A ms
- Memory Used: 8.740066528320312 MB
- Average CPU Usage: N/A%

---

### Python - Ollama - standard

**File:** `metrics_python_ollama_20260104_010214.json`

**Test Information:**
- Language/Framework: Python / Python
- Provider: Ollama
- Model: ministral-3
- Test Mode: standard
- Endpoint: http://localhost:11434
- Timestamp: 2026-01-04T01:02:14.685407+00:00
- Warmup Successful: True

**Machine Information:**
- OS: Windows 11
- Architecture: AMD64
- Processors: 24 cores (32 logical)
- CPU Max Frequency: 3.2 GHz
- Total Memory: 31.71 GB
- Available Memory: 12.95 GB
- GPU: NVIDIA GeForce RTX 4070 Ti SUPER (16376 MiB)
- Python Version: 3.14.2

**Performance Metrics:**
- Total Iterations: 500
- Total Execution Time: 109968.57929229736 ms
- Average Time per Iteration: 218.1537628173828 ms
- Min Iteration Time: 133.5597038269043 ms
- Max Iteration Time: 441.27917289733887 ms
- Median Iteration Time: N/A
- Standard Deviation: N/A ms
- Memory Used: 4.87109375 MB
- Average CPU Usage: N/A%

---
