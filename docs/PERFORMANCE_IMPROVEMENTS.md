# Performance Testing Improvements Guide

## Overview

This document describes the enhanced performance testing capabilities added to the MAF-PerformanceComparison project. These improvements provide more accurate, detailed, and statistically significant performance measurements for comparing .NET and Python implementations of the Microsoft Agent Framework.

## What Was Improved

### 1. Enhanced Memory Tracking

#### Previous Implementation
- Basic GC memory tracking in .NET: `GC.GetTotalMemory()`
- Basic process memory in Python: `psutil.Process().memory_info().rss`
- Single measurement at start and end
- No detailed memory breakdown

#### New Implementation
- **Process-level memory metrics**:
  - Working Set / RSS (physical memory actually used)
  - Private Memory / VMS (virtual memory size)
  - Peak memory usage during test execution
  - Shared memory (Linux/macOS)
  
- **Managed memory tracking** (.NET):
  - Managed heap size
  - GC generation-specific collection counts (Gen0, Gen1, Gen2)
  - Total GC pause time (important for latency-sensitive applications)
  
- **Continuous monitoring**:
  - Memory snapshots taken throughout test execution
  - Tracks memory growth over time
  - Identifies memory leaks and allocation patterns

**Why This Matters**: The previous approach only measured managed memory changes, which doesn't reflect the true memory footprint of the application. Process-level metrics show actual OS memory usage including native allocations, which is critical for fair cross-platform comparisons.

### 2. Accurate CPU Tracking

#### Previous Implementation
- Manual CPU sampling with basic averaging
- Inconsistent measurement intervals
- No thread-level awareness

#### New Implementation
- **Process CPU time tracking**:
  - Uses OS-provided CPU time counters
  - Calculates percentage based on wall clock time and core count
  - Accounts for multi-core systems properly
  
- **Periodic sampling**:
  - Regular snapshots throughout test execution
  - Tracks CPU usage patterns over time
  - Captures peak CPU usage
  
- **Thread awareness**:
  - Tracks thread count changes
  - Useful for identifying thread pool growth

**Why This Matters**: CPU percentage calculations based on instantaneous samples can be misleading. The new approach uses accumulated CPU time, which provides accurate measurements even on systems with varying CPU frequencies and competing processes.

### 3. Statistical Rigor

#### Previous Implementation
- Mean, min, max only
- No outlier detection
- No confidence intervals

#### New Implementation
- **Comprehensive statistics**:
  - Mean, median (more robust than mean)
  - Min, max
  - Standard deviation
  - Percentiles: P90, P95, P99
  
- **Distribution analysis**:
  - Identifies outliers
  - Shows performance consistency
  - Enables meaningful comparisons

**Why This Matters**: Mean alone is insufficient for performance analysis. A single outlier can skew results. Percentiles (especially P95/P99) are industry-standard metrics for understanding real-world performance under varying conditions.

### 4. Garbage Collection Insights

#### Previous Implementation
- No GC metrics tracked

#### New Implementation
- **Generation-specific collection counts**:
  - Gen0 (nursery) collections
  - Gen1 (mid-life) collections  
  - Gen2 (full) collections
  
- **GC pause time tracking** (.NET):
  - Total time spent in GC pauses
  - Critical for understanding latency impact
  
- **Python GC tracking**:
  - Generation-based GC counts
  - Helps identify collection pressure

**Why This Matters**: GC behavior significantly impacts performance and can explain latency spikes. High Gen2 collection counts indicate memory pressure. Understanding GC patterns is essential for performance tuning.

## Using the Enhanced Metrics

### .NET (C#) Implementation

```csharp
using PerformanceUtils;

// Create metrics tracker
var metrics = new PerformanceMetrics();

// Start measurement (includes clean GC baseline)
metrics.Start();

// Run your test iterations
for (int i = 0; i < ITERATIONS; i++)
{
    var iterationStart = Stopwatch.GetTimestamp();
    
    // Your test code here
    await agent.RunAsync("Test query");
    
    var iterationEnd = Stopwatch.GetTimestamp();
    var iterationTimeMs = (iterationEnd - iterationStart) * 1000.0 / Stopwatch.Frequency;
    metrics.RecordMeasurement(iterationTimeMs);
    
    // Capture snapshots periodically (e.g., every 100 iterations)
    if ((i + 1) % 100 == 0)
    {
        metrics.CaptureMemorySnapshot();
        metrics.CaptureCpuSnapshot();
    }
}

// Get comprehensive results
var result = metrics.GetResult();

// Access metrics
Console.WriteLine($"Mean: {result.Mean:F3} ms");
Console.WriteLine($"Median: {result.Median:F3} ms");
Console.WriteLine($"P95: {result.P95:F3} ms");
Console.WriteLine($"P99: {result.P99:F3} ms");
Console.WriteLine($"StdDev: {result.StandardDeviation:F3} ms");
Console.WriteLine($"Memory Delta: {result.WorkingSetDeltaMB:F2} MB");
Console.WriteLine($"GC Gen0/1/2: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");
Console.WriteLine($"GC Pause Time: {result.TotalGCPauseTimeMs:F2} ms");
Console.WriteLine($"Avg CPU: {result.AverageCpuPercent:F2}%");
```

### Python Implementation

```python
from performance_utils import PerformanceMetrics

# Create metrics tracker
metrics = PerformanceMetrics()

# Start measurement (includes clean GC baseline)
metrics.start()

# Run your test iterations
for i in range(ITERATIONS):
    iteration_start = time.perf_counter()
    
    # Your test code here
    await agent.run("Test query")
    
    iteration_end = time.perf_counter()
    iteration_time_ms = (iteration_end - iteration_start) * 1000
    metrics.record_measurement(iteration_time_ms)
    
    # Capture snapshots periodically (e.g., every 100 iterations)
    if (i + 1) % 100 == 0:
        metrics.capture_memory_snapshot()
        metrics.capture_cpu_snapshot()

# Get comprehensive results
result = metrics.get_result()

# Access metrics
print(f"Mean: {result.mean:.3f} ms")
print(f"Median: {result.median:.3f} ms")
print(f"P95: {result.p95:.3f} ms")
print(f"P99: {result.p99:.3f} ms")
print(f"StdDev: {result.stdev:.3f} ms")
print(f"RSS Delta: {result.rss_delta_mb:.2f} MB")
print(f"GC Gen0/1/2: {result.gc_gen0_collections}/{result.gc_gen1_collections}/{result.gc_gen2_collections}")
print(f"Avg CPU: {result.average_cpu_percent:.2f}%")
```

## Metrics Export Format

The enhanced metrics are exported in a structured JSON format:

```json
{
  "TestInfo": {
    "Language": "CSharp",
    "Framework": "DotNet",
    "Provider": "Ollama",
    "Timestamp": "2026-01-05T15:30:00Z"
  },
  "Metrics": {
    "TotalIterations": 1000,
    "TotalExecutionTimeMs": 125340,
    
    "Statistics": {
      "Mean": 125.34,
      "Median": 123.45,
      "Min": 98.21,
      "Max": 456.78,
      "P90": 145.23,
      "P95": 167.89,
      "P99": 234.56,
      "StandardDeviation": 23.45
    },
    
    "Memory": {
      "WorkingSetDeltaMB": 45.67,
      "PrivateMemoryDeltaMB": 52.34,
      "PeakWorkingSetMB": 256.78,
      "PeakPrivateMemoryMB": 298.45
    },
    
    "GarbageCollection": {
      "Gen0Collections": 234,
      "Gen1Collections": 45,
      "Gen2Collections": 3,
      "TotalPauseTimeMs": 456.78
    },
    
    "CPU": {
      "AveragePercent": 23.45,
      "MaxPercent": 45.67
    },
    
    "DetailedSnapshots": {
      "MemorySnapshots": [...],
      "CpuSnapshots": [...]
    }
  }
}
```

## Interpreting the Metrics

### Timing Metrics

- **Mean**: Average response time - good for overall throughput assessment
- **Median**: Middle value - better indicator of "typical" performance, resistant to outliers
- **P95/P99**: 95th/99th percentile - critical for SLA compliance, shows worst-case scenarios
- **Standard Deviation**: Consistency measure - lower is better, high values indicate variable performance

### Memory Metrics

- **Working Set / RSS**: Actual physical memory used - most important for capacity planning
- **Private Memory / VMS**: Total virtual memory - shows allocation patterns
- **Peak Memory**: Maximum memory used during test - important for resource limits
- **Memory Delta**: Growth during test - positive values may indicate memory leaks

### GC Metrics

- **Gen0 Collections**: Frequent, low-cost - high counts are normal
- **Gen1 Collections**: Medium cost - moderate counts expected
- **Gen2 Collections**: Expensive, full heap scan - should be rare (< 10 for 1000 iterations)
- **GC Pause Time**: Total time frozen for GC - impacts latency, should be < 5% of total time

### CPU Metrics

- **Average CPU %**: Overall CPU efficiency - compare between implementations
- **Max CPU %**: Peak load - important for burst scenarios
- **Thread Count**: Concurrency level - increasing count may indicate thread pool growth

## Best Practices

### 1. Consistent Test Environment
- Run tests on the same machine
- Close unnecessary applications
- Disable CPU frequency scaling if possible
- Use same number of iterations for fair comparison

### 2. Multiple Runs
- Run tests at least 3 times
- Average the results
- Check for consistency between runs
- Investigate large variations

### 3. Warmup Phases
- Always include warmup iterations (not counted in metrics)
- Allows JIT compilation and optimization
- Stabilizes memory allocations
- The enhanced metrics tracker includes GC-based warmup

### 4. Snapshot Frequency
- Capture snapshots regularly but not too frequently
- Every 50-100 iterations is usually sufficient
- More frequent snapshots add overhead
- Balance between detail and accuracy

### 5. Analyzing Results
- Don't rely on mean alone - use percentiles
- Look for memory growth trends in snapshots
- High GC counts may indicate memory pressure
- CPU spikes may indicate contention or background work

## Troubleshooting

### High P99 Latencies
- Check for GC pauses (Gen2 collections)
- Look for CPU spikes in snapshots
- May indicate network issues or timeouts
- Consider increasing warmup iterations

### Increasing Memory Usage
- Check memory snapshots for steady growth
- High Gen2 collections indicate pressure
- May indicate memory leaks or caching
- Compare RSS vs managed memory

### Variable Performance
- High standard deviation indicates inconsistency
- Check CPU snapshots for competing processes
- May need to pin process to specific cores
- Consider longer warmup phase

### CPU Not Scaling
- Compare thread count across snapshots
- May indicate lock contention
- Check for sequential bottlenecks
- Consider async/await patterns

## Future Enhancements

Potential areas for further improvement:

1. **BenchmarkDotNet Integration** (.NET)
   - Industry-standard benchmarking framework
   - Automatic outlier detection
   - Statistical significance testing

2. **pytest-benchmark Integration** (Python)
   - Standardized benchmark framework
   - Comparison with baselines
   - Regression detection

3. **Network Metrics**
   - Bytes sent/received
   - Request/response sizes
   - Network latency isolation

4. **I/O Metrics**
   - Disk read/write operations
   - File system cache impact

5. **Process Affinity**
   - Pin to specific CPU cores
   - Reduce scheduler variance
   - More consistent results

## References

- [.NET Performance Best Practices](https://learn.microsoft.com/en-us/dotnet/framework/performance/)
- [Python Performance Tips](https://wiki.python.org/moin/PythonSpeed/PerformanceTips)
- [psutil Documentation](https://psutil.readthedocs.io/)
- [Understanding GC Metrics](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/)
