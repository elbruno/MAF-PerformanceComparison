# Performance Comparison Report

This report compares two performance test results from the Microsoft Agent Framework.

**Test Run Date:** January 3, 2026

## Test Configuration Comparison

- **First (C#/.NET)**
  - Language/Framework: CSharp / DotNet
  - Provider: Ollama
  - Model: ministral-3
  - Endpoint: <http://localhost:11434>
  - Timestamp: 2026-01-03T16:59:41.5475899+00:00
  - WarmupSuccessful: true

- **Second (Python)**
  - Language/Framework: Python / Python
  - Provider: Ollama
  - Model: ministral-3
  - Endpoint: <http://localhost:11434>
  - Timestamp: 2026-01-03T17:03:11.445067+00:00
  - WarmupSuccessful: true

## Performance Metrics

| Metric | C#/.NET | Python | Difference | Percentage |
|---|---:|---:|---:|---:|
| TotalIterations | 1,000 | 1,000 | 0 | 0% |
| TotalExecutionTimeMs | 192,048 | 203,577.2235 | 11,529.2235 ms slower (Python) | 6.00% |
| AverageTimePerIterationMs | 188.5350 | 202.4442 | 13.9092 ms slower (Python) | 7.38% |
| MinIterationTimeMs | 123.1511 | 129.0312 | C# faster by 5.88 ms | 4.77% |
| MaxIterationTimeMs | 859.6458 | 454.2198 | C# has outliers 405.43 ms higher | 89.17% |
| Range (Max-Min) | 736.4947 | 325.1886 | C# larger by 411.31 ms | 126.50% |
| MemoryUsedMB | 16.2168 | 2.0625 | C# uses 14.1543 MB more | 686.35% |

## Analysis

### Speed Performance

- **Total execution time:** C#/.NET is faster by ~11.53 seconds (≈6.0%)
- **Per-iteration average:** C#/.NET is faster by ~13.91 ms (≈7.4%)
- **Winner:** C#/.NET has better average throughput

### Consistency & Stability

- **Python** shows significantly tighter latency distribution:
  - Max: 454 ms vs C#'s 859 ms
  - Range: 325 ms vs C#'s 736 ms
- **C#** exhibits pronounced outliers with max response 89% higher than Python's max
- **Winner:** Python demonstrates more predictable, stable performance

### Resource Efficiency

- **Memory:** C# uses ≈16.22 MB vs Python's ≈2.06 MB
- Python uses ~87% less memory (6.86x more efficient)
- **Winner:** Python is significantly more memory-efficient

## Key Insights

### Performance Characteristics

1. **C#/.NET Advantages:**
   - ~7.4% faster average iteration time
   - ~6.0% faster total execution time
   - Better throughput for batch processing

2. **Python Advantages:**
   - 87% less memory consumption (critical for scaling)
   - More consistent performance (max latency 47% lower)
   - 126% narrower latency range (more predictable)

3. **Notable Observations:**
   - Both implementations completed successful warmup
   - Tests ran ~3.5 minutes apart under similar conditions
   - C# shows occasional large outliers (859 ms max vs 454 ms for Python)
   - Python's consistency suggests better suitability for real-time/latency-sensitive applications

## Recommendations

### Choose C#/.NET if

- Raw throughput is the primary concern
- Running single-instance or few concurrent agents
- Memory is abundant
- Batch processing workloads where average latency matters more than worst-case

### Choose Python if

- Running many concurrent agent instances (memory constrained)
- Predictable latency is critical (SLA-driven workloads)
- Cost optimization for cloud deployments (memory costs)
- Real-time or user-facing applications where consistency matters

### Optimization Opportunities

**For C# (reduce outliers):**

1. Profile garbage collection events during test runs
2. Tune GC settings (Server GC, concurrent GC)
3. Increase warmup iterations to stabilize JIT compilation
4. Monitor for thread pool starvation or async bottlenecks
5. Consider using `GC.TryStartNoGCRegion()` for critical paths

**For Python (improve speed):**

1. Investigate async/await optimization opportunities
2. Consider using PyPy for JIT compilation benefits
3. Profile for I/O bottlenecks in the agent framework
4. Explore connection pooling if HTTP requests are involved

## Summary

**Overall Verdict:** No single winner—choice depends on requirements.

- **Speed Champion:** C#/.NET (7.4% faster average, 6.0% faster total)
- **Efficiency Champion:** Python (87% less memory, 126% more consistent)
- **Production Recommendation:**
  - High-throughput batch systems → C#/.NET
  - Scalable microservices/concurrent agents → Python
  - Hybrid approach: Use both based on specific workload characteristics

---

_Report generated: 2026-01-03_  
_C# Test: metrics_dotnet_ollama_20260103_165941.json_  
_Python Test: metrics_python_ollama_20260103_170311.json_
