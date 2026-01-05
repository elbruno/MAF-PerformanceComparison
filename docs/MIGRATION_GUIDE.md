# Migration Guide: Applying Enhanced Performance Metrics

This guide shows how to update existing agent implementations to use the enhanced performance metrics libraries.

## Quick Reference

### .NET Agents

**Step 1**: Add project reference to `.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\PerformanceUtils\PerformanceUtils.csproj" />
</ItemGroup>
```

**Step 2**: Add using statement:
```csharp
using PerformanceUtils;
```

**Step 3**: Replace old initialization:
```csharp
// OLD
var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);
var iterationTimes = new List<double>();

// NEW
var performanceMetrics = new PerformanceMetrics();
performanceMetrics.Start();
```

**Step 4**: Update measurement recording:
```csharp
// OLD
iterationTimes.Add(iterationTimeMs);

// NEW
performanceMetrics.RecordMeasurement(iterationTimeMs);

// Add periodic snapshots (every 100 iterations)
if ((i + 1) % 100 == 0)
{
    performanceMetrics.CaptureMemorySnapshot();
    performanceMetrics.CaptureCpuSnapshot();
}
```

**Step 5**: Replace metrics calculation:
```csharp
// OLD
var avgIterationTime = iterationTimes.Average();
var minIterationTime = iterationTimes.Min();
var maxIterationTime = iterationTimes.Max();
stopwatch.Stop();
var endMemory = GC.GetTotalMemory(false);
var memoryUsed = (endMemory - startMemory) / 1024.0 / 1024.0;

// NEW
var result = performanceMetrics.GetResult();
// Access: result.Mean, result.Median, result.P95, result.WorkingSetDeltaMB, etc.
```

**Step 6**: Update JSON export:
```csharp
// Add to Metrics section
Metrics = new
{
    TotalIterations = ITERATIONS,
    TotalExecutionTimeMs = result.TotalElapsedMs,
    
    Statistics = new
    {
        Mean = result.Mean,
        Median = result.Median,
        Min = result.Min,
        Max = result.Max,
        P90 = result.P90,
        P95 = result.P95,
        P99 = result.P99,
        StandardDeviation = result.StandardDeviation
    },
    
    Memory = new
    {
        WorkingSetDeltaMB = result.WorkingSetDeltaMB,
        PrivateMemoryDeltaMB = result.PrivateMemoryDeltaMB,
        PeakWorkingSetMB = result.PeakWorkingSetMB,
        PeakPrivateMemoryMB = result.PeakPrivateMemoryMB
    },
    
    GarbageCollection = new
    {
        Gen0Collections = result.Gen0Collections,
        Gen1Collections = result.Gen1Collections,
        Gen2Collections = result.Gen2Collections,
        TotalGCPauseTimeMs = result.TotalGCPauseTimeMs
    },
    
    CPU = new
    {
        AveragePercent = result.AverageCpuPercent,
        MaxPercent = result.MaxCpuPercent
    },
    
    // Keep legacy fields for backward compatibility
    AverageTimePerIterationMs = result.Mean,
    MinIterationTimeMs = result.Min,
    MaxIterationTimeMs = result.Max,
    MemoryUsedMB = result.WorkingSetDeltaMB
}
```

### Python Agents

**Step 1**: Add import path (if in subdirectory):
```python
import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))
```

**Step 2**: Import module:
```python
from performance_utils import PerformanceMetrics
```

**Step 3**: Replace old initialization:
```python
# OLD
start_time = time.time()
process = psutil.Process(os.getpid())
start_memory = process.memory_info().rss / 1024 / 1024
iteration_times = []

# NEW
performance_metrics = PerformanceMetrics()
performance_metrics.start()
```

**Step 4**: Update measurement recording:
```python
# OLD
iteration_times.append((iteration_end - iteration_start) * 1000)

# NEW
performance_metrics.record_measurement(iteration_time_ms)

# Add periodic snapshots (every 100 iterations)
if (i + 1) % 100 == 0:
    performance_metrics.capture_memory_snapshot()
    performance_metrics.capture_cpu_snapshot()
```

**Step 5**: Replace metrics calculation:
```python
# OLD
avg_iteration_time = sum(iteration_times) / len(iteration_times)
min_iteration_time = min(iteration_times)
max_iteration_time = max(iteration_times)
end_time = time.time()
end_memory = process.memory_info().rss / 1024 / 1024
total_execution_time = (end_time - start_time) * 1000
memory_used = end_memory - start_memory

# NEW
result = performance_metrics.get_result()
# Access: result.mean, result.median, result.p95, result.rss_delta_mb, etc.
```

**Step 6**: Update JSON export:
```python
# Add to Metrics section
"Metrics": {
    "TotalIterations": ITERATIONS,
    "TotalExecutionTimeMs": result.total_elapsed_ms,
    
    "Statistics": {
        "Mean": result.mean,
        "Median": result.median,
        "Min": result.min,
        "Max": result.max,
        "P90": result.p90,
        "P95": result.p95,
        "P99": result.p99,
        "StandardDeviation": result.stdev
    },
    
    "Memory": {
        "RSSDeltaMB": result.rss_delta_mb,
        "VMSDeltaMB": result.vms_delta_mb,
        "PeakRSSMB": result.peak_rss_mb,
        "PeakVMSMB": result.peak_vms_mb
    },
    
    "GarbageCollection": {
        "Gen0Collections": result.gc_gen0_collections,
        "Gen1Collections": result.gc_gen1_collections,
        "Gen2Collections": result.gc_gen2_collections
    },
    
    "CPU": {
        "AveragePercent": result.average_cpu_percent,
        "MaxPercent": result.max_cpu_percent
    },
    
    # Keep legacy fields for backward compatibility
    "AverageTimePerIterationMs": result.mean,
    "MinIterationTimeMs": result.min,
    "MaxIterationTimeMs": result.max,
    "MemoryUsedMB": result.rss_delta_mb
}
```

## Example: Complete Before/After

### .NET Example (Simplified)

**Before**:
```csharp
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

var stopwatch = Stopwatch.StartNew();
var startMemory = GC.GetTotalMemory(true);
var iterationTimes = new List<double>();

for (int i = 0; i < ITERATIONS; i++)
{
    var iterStart = Stopwatch.GetTimestamp();
    
    // Your test code
    await DoWork();
    
    var iterEnd = Stopwatch.GetTimestamp();
    var timeMs = (iterEnd - iterStart) * 1000.0 / Stopwatch.Frequency;
    iterationTimes.Add(timeMs);
}

stopwatch.Stop();
var endMemory = GC.GetTotalMemory(false);
var avgTime = iterationTimes.Average();
var minTime = iterationTimes.Min();
var maxTime = iterationTimes.Max();
var memUsed = (endMemory - startMemory) / 1024.0 / 1024.0;

Console.WriteLine($"Avg: {avgTime:F3} ms, Memory: {memUsed:F2} MB");
```

**After**:
```csharp
using System;
using System.Diagnostics;
using PerformanceUtils;

var metrics = new PerformanceMetrics();
metrics.Start();

for (int i = 0; i < ITERATIONS; i++)
{
    var iterStart = Stopwatch.GetTimestamp();
    
    // Your test code
    await DoWork();
    
    var iterEnd = Stopwatch.GetTimestamp();
    var timeMs = (iterEnd - iterStart) * 1000.0 / Stopwatch.Frequency;
    metrics.RecordMeasurement(timeMs);
    
    if ((i + 1) % 100 == 0)
    {
        metrics.CaptureMemorySnapshot();
        metrics.CaptureCpuSnapshot();
    }
}

var result = metrics.GetResult();

Console.WriteLine($"Mean: {result.Mean:F3} ms, Median: {result.Median:F3} ms");
Console.WriteLine($"P95: {result.P95:F3} ms, P99: {result.P99:F3} ms");
Console.WriteLine($"Memory: {result.WorkingSetDeltaMB:F2} MB");
Console.WriteLine($"GC: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");
Console.WriteLine($"CPU: {result.AverageCpuPercent:F2}%");
```

### Python Example (Simplified)

**Before**:
```python
import time
import psutil
import os

start_time = time.time()
process = psutil.Process(os.getpid())
start_memory = process.memory_info().rss / 1024 / 1024
iteration_times = []

for i in range(ITERATIONS):
    iter_start = time.perf_counter()
    
    # Your test code
    await do_work()
    
    iter_end = time.perf_counter()
    iteration_times.append((iter_end - iter_start) * 1000)

end_time = time.time()
end_memory = process.memory_info().rss / 1024 / 1024
total_time = (end_time - start_time) * 1000
avg_time = sum(iteration_times) / len(iteration_times)
min_time = min(iteration_times)
max_time = max(iteration_times)
mem_used = end_memory - start_memory

print(f"Avg: {avg_time:.3f} ms, Memory: {mem_used:.2f} MB")
```

**After**:
```python
import time
from performance_utils import PerformanceMetrics

metrics = PerformanceMetrics()
metrics.start()

for i in range(ITERATIONS):
    iter_start = time.perf_counter()
    
    # Your test code
    await do_work()
    
    iter_end = time.perf_counter()
    time_ms = (iter_end - iter_start) * 1000
    metrics.record_measurement(time_ms)
    
    if (i + 1) % 100 == 0:
        metrics.capture_memory_snapshot()
        metrics.capture_cpu_snapshot()

result = metrics.get_result()

print(f"Mean: {result.mean:.3f} ms, Median: {result.median:.3f} ms")
print(f"P95: {result.p95:.3f} ms, P99: {result.p99:.3f} ms")
print(f"Memory: {result.rss_delta_mb:.2f} MB")
print(f"GC: {result.gc_gen0_collections}/{result.gc_gen1_collections}/{result.gc_gen2_collections}")
print(f"CPU: {result.average_cpu_percent:.2f}%")
```

## Handling Complex Test Modes

For agents with multiple test modes (like HelloWorldAgent), apply the pattern to each test function:

```csharp
async Task RunStandardTest(PerformanceMetrics metrics, int iterations)
{
    for (int i = 0; i < iterations; i++)
    {
        var start = Stopwatch.GetTimestamp();
        await DoWork();
        var end = Stopwatch.GetTimestamp();
        var timeMs = (end - start) * 1000.0 / Stopwatch.Frequency;
        metrics.RecordMeasurement(timeMs);
        
        if ((i + 1) % 100 == 0)
        {
            metrics.CaptureMemorySnapshot();
            metrics.CaptureCpuSnapshot();
        }
    }
}

async Task RunBatchTest(PerformanceMetrics metrics, int iterations, int batchSize)
{
    for (int i = 0; i < iterations; i += batchSize)
    {
        var batchStart = Stopwatch.GetTimestamp();
        
        var tasks = new List<Task>();
        for (int j = 0; j < batchSize; j++)
            tasks.Add(DoWork());
        await Task.WhenAll(tasks);
        
        var batchEnd = Stopwatch.GetTimestamp();
        var batchTimeMs = (batchEnd - batchStart) * 1000.0 / Stopwatch.Frequency;
        var perItemMs = batchTimeMs / batchSize;
        
        for (int j = 0; j < batchSize; j++)
            metrics.RecordMeasurement(perItemMs);
        
        metrics.CaptureMemorySnapshot();
        metrics.CaptureCpuSnapshot();
    }
}

// Main code
var metrics = new PerformanceMetrics();
metrics.Start();

switch (testMode)
{
    case "standard":
        await RunStandardTest(metrics, ITERATIONS);
        break;
    case "batch":
        await RunBatchTest(metrics, ITERATIONS, BATCH_SIZE);
        break;
    // ... other modes
}

var result = metrics.GetResult();
```

## Testing the Migration

After applying changes:

1. **Build/syntax check**:
   ```bash
   # .NET
   dotnet build
   
   # Python
   python3 -m py_compile main.py
   ```

2. **Run with small iteration count**:
   ```bash
   # .NET
   ITERATIONS=10 dotnet run
   
   # Python
   ITERATIONS=10 python3 main.py
   ```

3. **Verify JSON output**:
   - Check for `Statistics`, `Memory`, `GarbageCollection`, `CPU` sections
   - Verify percentile values are present
   - Confirm memory metrics are non-zero

4. **Compare with baseline**:
   - Run old version and new version with same ITERATIONS
   - Legacy fields should match closely
   - New metrics provide additional insights

## Troubleshooting

**Issue**: Build error "PerformanceUtils not found"
- Solution: Ensure project reference path is correct relative to agent directory

**Issue**: Python import error
- Solution: Check sys.path.insert points to correct parent directory

**Issue**: Metrics show zero values
- Solution: Ensure `Start()` is called before recording measurements

**Issue**: CPU percentage seems wrong
- Solution: Verify snapshots are captured (not too frequent, not too infrequent)

## Next Steps

After migrating all agents:

1. Update `process_results_ollama.py` to parse new metrics format
2. Create visualization for percentiles and distributions
3. Add statistical comparison between .NET and Python
4. Generate trend charts from snapshot data
5. Document interpretation guidelines for new metrics

## Questions?

Refer to:
- `docs/PERFORMANCE_IMPROVEMENTS.md` - Detailed explanation of improvements
- `docs/PERFORMANCE_ENHANCEMENTS_SUMMARY.md` - Implementation summary
- Source code: `dotnet/PerformanceUtils/` and `python/performance_utils/`
