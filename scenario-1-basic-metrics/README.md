# Scenario 1: Basic Performance Metrics (Learning Phase)

## Overview

This scenario represents the **original/basic implementation** of performance testing in the MAF-PerformanceComparison project. It serves as a learning phase and baseline for understanding basic performance measurement approaches.

## What's Included

This implementation uses **basic performance metrics**:

### Timing
- Simple stopwatch timing
- Manual iteration time collection
- Basic statistics: mean, min, max

### Memory
- **. NET**: `GC.GetTotalMemory()` (managed heap only)
- **Python**: Basic `psutil` RSS measurement
- Start and end snapshots only

### CPU
- Manual CPU sampling
- Inconsistent measurement intervals
- No multi-core normalization

## Limitations

This approach has several limitations that make it insufficient for accurate performance comparisons:

1. **Memory Tracking**
   - Only tracks managed memory in .NET (misses native allocations)
   - Single RSS measurement in Python (no memory breakdown)
   - No peak memory tracking
   - No trend analysis

2. **CPU Monitoring**
   - Instantaneous CPU samples (unreliable)
   - No proper multi-core accounting
   - High variance in measurements

3. **Statistical Analysis**
   - Only mean, min, max (no percentiles)
   - No standard deviation
   - Outliers not identified

4. **Missing Insights**
   - No garbage collection metrics
   - No correlation between latency and GC events
   - No understanding of memory pressure

## Example Output

```
Total Iterations: 1000
Total Execution Time: 125340 ms
Average Time per Iteration: 125.34 ms
Min Iteration Time: 98.21 ms
Max Iteration Time: 456.78 ms
Memory Used: 45.67 MB
```

## Why This is a Learning Phase

This implementation demonstrates **what not to do** for production-grade performance testing:

- ❌ Inaccurate memory measurements
- ❌ Unreliable CPU tracking
- ❌ Insufficient statistical analysis
- ❌ No diagnostic capabilities
- ❌ Poor cross-platform comparability

## Moving to Scenario 2

For **accurate, production-ready performance testing**, see:
- **[Scenario 2: Enhanced Metrics](../scenario-2-enhanced-metrics/README.md)** - Industry-standard implementation

Scenario 2 provides:
- ✅ Process-level memory tracking
- ✅ Accurate CPU monitoring
- ✅ Statistical rigor (P90, P95, P99)
- ✅ GC insights
- ✅ Trend analysis

## Usage

### .NET

```bash
cd dotnet/OllamaAgent
dotnet build
ITERATIONS=1000 dotnet run
```

### Python

```bash
cd python/ollama_agent
pip install -r requirements.txt
ITERATIONS=1000 python main.py
```

## Learning Points

By comparing this scenario with Scenario 2, you can learn:

1. **Why process-level memory matters** vs managed-only
2. **Why time-based CPU calculation is more accurate** than instantaneous samples
3. **Why percentiles (P95, P99) are critical** for SLA planning
4. **Why GC metrics explain latency** variations
5. **Why trend analysis reveals** memory leaks and degradation

## References

- See [Scenario 2](../scenario-2-enhanced-metrics/README.md) for the enhanced implementation
- See [Performance Improvements Guide](../docs/PERFORMANCE_IMPROVEMENTS.md) for technical details
- See [Main README](../README.md) for project overview
