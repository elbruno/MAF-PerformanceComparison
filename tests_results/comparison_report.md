# Performance Comparison Report

This report compares two performance test results from the Microsoft Agent Framework.

## Test Configuration Comparison

- **First (C#/.NET)**
  - Language/Framework: CSharp / DotNet
  - Provider: Ollama
  - Model: ministral-3
  - Endpoint: <http://localhost:11434>
  - Timestamp: 2026-01-03T16:44:48.6552059+00:00
  - WarmupSuccessful: true

- **Second (Python)**
  - Language/Framework: Python / Python
  - Provider: Ollama
  - Model: ministral-3
  - Endpoint: <http://localhost:11434>
  - Timestamp: 2026-01-03T16:48:44.735982+00:00
  - WarmupSuccessful: true

## Performance Metrics

| Metric | C#/.NET (First) | Python (Second) | Difference | Percentage |
|---|---:|---:|---:|---:|
| TotalIterations | 1000 | 1000 | 0 | 0% |
| TotalExecutionTimeMs | 193,340 | 204,568.1236 | 11,228.1236 slower (Python) | 5.81% |
| AverageTimePerIterationMs | 192.6916 | 203.7171 | 11.0255 slower (Python) | 5.72% |
| MinIterationTimeMs | 122.2108 | 133.3435 | C# faster by 11.13 ms | 9.17% |
| MaxIterationTimeMs | 957.1436 | 516.1841 | C# slower by 440.9595 ms (outliers) | 85.47% |
| Range (Max-Min) | 834.9328 | 382.8406 | C# larger by 452.0922 ms | 118.11% |
| MemoryUsedMB | 16.23121643 | 1.9609375 | C# uses 14.2703 MB more | 728.09% |

## Analysis

- Total execution time: C#/.NET is faster by ~11.23 seconds (≈5.8%).
- Per-iteration average: C#/.NET is faster by ~11.03 ms (≈5.7%).
- Consistency: Python shows a tighter latency distribution (max 516 ms) vs C# with large spikes (max 957 ms). C# range is ~835 ms vs Python ~383 ms.
- Memory: C# uses substantially more memory (≈16.23 MB) vs Python (≈1.96 MB).

## Key Insights

- If raw average speed is the priority, C#/.NET performs better (~5.7–5.8% faster).
- If stability (lower max latency and narrower range) and low memory footprint are priorities, Python is preferable.
- C# shows pronounced outliers; investigate JIT/GC or other system-level events.

## Recommendations

1. Run multiple trials and collect percentiles (p50/p90/p95/p99) to understand distribution.
2. Collect per-iteration timings and system metrics (GC, CPU, memory, disk, network) to find root cause of C# outliers.
3. If memory is a bottleneck (many concurrent instances), prefer Python or optimize C# memory.
4. If latency is primary, optimize C# to reduce outliers (tune GC, warmup, profiling).

## Summary

- C# wins on average performance; Python wins on memory and consistency.
- No absolute winner — choose based on your operational priorities.

---

_Report generated: 2026-01-03T (automated summary)._
