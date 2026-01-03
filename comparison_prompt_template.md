# Performance Comparison Prompt Template

Use this prompt to compare two performance test result files with an LLM.

## Prompt Template

```
I have two performance test results from running the Microsoft Agent Framework in different environments. Please analyze and compare these results.

**First Test Result:**
```json
[PASTE CONTENT OF FIRST METRICS FILE HERE]
```

**Second Test Result:**
```json
[PASTE CONTENT OF SECOND METRICS FILE HERE]
```

Please provide a comprehensive comparison that includes:

1. **Test Configuration Comparison:**
   - Language/Framework used (C#/.NET vs Python)
   - AI Provider and Model
   - Warmup status
   - Test timestamp

2. **Performance Metrics Analysis:**
   - Total execution time comparison (which is faster and by what percentage?)
   - Average time per iteration (which has better average performance?)
   - Min/Max iteration times (which has more consistent performance?)
   - Performance variability (compare min/max ranges)

3. **Resource Usage:**
   - Memory consumption comparison
   - Efficiency analysis (performance vs resource usage)

4. **Key Insights:**
   - Which implementation performs better overall?
   - What are the notable differences?
   - Any recommendations based on the results?

5. **Statistical Summary:**
   - Provide a clear winner or note if results are comparable
   - Highlight any significant performance gaps
   - Consider both speed and resource efficiency

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

Each metrics JSON file contains:

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

- **Consider warmup status**: Tests with successful warmups may show different performance characteristics
- **Check timestamps**: Ensure tests were run under similar conditions (similar system load, network conditions, etc.)
- **Look at consistency**: Min/Max ranges indicate performance stability
- **Memory matters**: Lower memory usage is better for scalability
- **Average vs Total**: Both metrics are important - average shows per-operation efficiency, total shows cumulative overhead
