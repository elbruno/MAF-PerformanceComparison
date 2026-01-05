using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerformanceUtils
{
    /// <summary>
    /// Enhanced performance metrics tracker providing accurate memory, CPU, and statistical measurements.
    /// </summary>
    public class PerformanceMetrics
    {
        private readonly Process _process;
        private readonly Stopwatch _stopwatch;
        private readonly List<double> _measurements;
        private readonly List<MemorySnapshot> _memorySnapshots;
        private readonly List<CpuSnapshot> _cpuSnapshots;
        
        private long _startWorkingSet;
        private long _startPrivateBytes;
        private DateTime _lastCpuCheck;
        private TimeSpan _lastCpuTime;
        private int _gcGen0Start;
        private int _gcGen1Start;
        private int _gcGen2Start;

        public PerformanceMetrics()
        {
            _process = Process.GetCurrentProcess();
            _stopwatch = new Stopwatch();
            _measurements = new List<double>();
            _memorySnapshots = new List<MemorySnapshot>();
            _cpuSnapshots = new List<CpuSnapshot>();
        }

        /// <summary>
        /// Start performance measurement session.
        /// </summary>
        public void Start()
        {
            // Force garbage collection to get clean baseline
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _process.Refresh();
            _startWorkingSet = _process.WorkingSet64;
            _startPrivateBytes = _process.PrivateMemorySize64;
            
            _gcGen0Start = GC.CollectionCount(0);
            _gcGen1Start = GC.CollectionCount(1);
            _gcGen2Start = GC.CollectionCount(2);
            
            _lastCpuCheck = DateTime.UtcNow;
            _lastCpuTime = _process.TotalProcessorTime;
            
            _stopwatch.Start();
        }

        /// <summary>
        /// Record a single measurement (e.g., one iteration time in milliseconds).
        /// </summary>
        public void RecordMeasurement(double valueMs)
        {
            _measurements.Add(valueMs);
        }

        /// <summary>
        /// Capture a memory snapshot at the current point in time.
        /// </summary>
        public void CaptureMemorySnapshot()
        {
            _process.Refresh();
            var snapshot = new MemorySnapshot
            {
                TimestampMs = _stopwatch.ElapsedMilliseconds,
                WorkingSetMB = _process.WorkingSet64 / 1024.0 / 1024.0,
                PrivateMemoryMB = _process.PrivateMemorySize64 / 1024.0 / 1024.0,
                ManagedMemoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0,
                Gen0Collections = GC.CollectionCount(0) - _gcGen0Start,
                Gen1Collections = GC.CollectionCount(1) - _gcGen1Start,
                Gen2Collections = GC.CollectionCount(2) - _gcGen2Start
            };
            _memorySnapshots.Add(snapshot);
        }

        /// <summary>
        /// Capture a CPU usage snapshot.
        /// </summary>
        public void CaptureCpuSnapshot()
        {
            _process.Refresh();
            var currentTime = DateTime.UtcNow;
            var currentCpuTime = _process.TotalProcessorTime;
            
            var wallTimeDiff = (currentTime - _lastCpuCheck).TotalMilliseconds;
            var cpuTimeDiff = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
            
            if (wallTimeDiff > 0)
            {
                var cpuPercent = (cpuTimeDiff / wallTimeDiff / Environment.ProcessorCount) * 100.0;
                
                var snapshot = new CpuSnapshot
                {
                    TimestampMs = _stopwatch.ElapsedMilliseconds,
                    CpuPercent = cpuPercent,
                    ThreadCount = _process.Threads.Count
                };
                _cpuSnapshots.Add(snapshot);
            }
            
            _lastCpuCheck = currentTime;
            _lastCpuTime = currentCpuTime;
        }

        /// <summary>
        /// Stop measurement and calculate comprehensive metrics.
        /// </summary>
        public MetricsResult GetResult()
        {
            _stopwatch.Stop();
            _process.Refresh();

            var result = new MetricsResult
            {
                TotalElapsedMs = _stopwatch.ElapsedMilliseconds,
                MeasurementCount = _measurements.Count
            };

            // Statistical measurements
            if (_measurements.Any())
            {
                var sorted = _measurements.OrderBy(x => x).ToList();
                result.Mean = _measurements.Average();
                result.Min = sorted.First();
                result.Max = sorted.Last();
                result.Median = GetPercentile(sorted, 0.5);
                result.P90 = GetPercentile(sorted, 0.90);
                result.P95 = GetPercentile(sorted, 0.95);
                result.P99 = GetPercentile(sorted, 0.99);
                result.StandardDeviation = CalculateStdDev(_measurements, result.Mean);
            }

            // Memory metrics
            result.WorkingSetDeltaMB = (_process.WorkingSet64 - _startWorkingSet) / 1024.0 / 1024.0;
            result.PrivateMemoryDeltaMB = (_process.PrivateMemorySize64 - _startPrivateBytes) / 1024.0 / 1024.0;
            result.PeakWorkingSetMB = _process.PeakWorkingSet64 / 1024.0 / 1024.0;
            result.PeakPrivateMemoryMB = _process.PeakPagedMemorySize64 / 1024.0 / 1024.0;

            // GC metrics
            result.Gen0Collections = GC.CollectionCount(0) - _gcGen0Start;
            result.Gen1Collections = GC.CollectionCount(1) - _gcGen1Start;
            result.Gen2Collections = GC.CollectionCount(2) - _gcGen2Start;
            result.TotalGCPauseTimeMs = GC.GetTotalPauseDuration().TotalMilliseconds;

            // CPU metrics
            if (_cpuSnapshots.Any())
            {
                result.AverageCpuPercent = _cpuSnapshots.Average(s => s.CpuPercent);
                result.MaxCpuPercent = _cpuSnapshots.Max(s => s.CpuPercent);
            }

            // Detailed snapshots
            result.MemorySnapshots = _memorySnapshots;
            result.CpuSnapshots = _cpuSnapshots;

            return result;
        }

        private static double GetPercentile(List<double> sortedValues, double percentile)
        {
            if (sortedValues.Count == 0) return 0;
            
            var index = percentile * (sortedValues.Count - 1);
            var lower = (int)Math.Floor(index);
            var upper = (int)Math.Ceiling(index);
            
            if (lower == upper) return sortedValues[lower];
            
            var fraction = index - lower;
            return sortedValues[lower] * (1 - fraction) + sortedValues[upper] * fraction;
        }

        private static double CalculateStdDev(List<double> values, double mean)
        {
            if (values.Count <= 1) return 0;
            
            var sumOfSquares = values.Sum(val => Math.Pow(val - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }
    }

    public class MemorySnapshot
    {
        public long TimestampMs { get; set; }
        public double WorkingSetMB { get; set; }
        public double PrivateMemoryMB { get; set; }
        public double ManagedMemoryMB { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
    }

    public class CpuSnapshot
    {
        public long TimestampMs { get; set; }
        public double CpuPercent { get; set; }
        public int ThreadCount { get; set; }
    }

    public class MetricsResult
    {
        public long TotalElapsedMs { get; set; }
        public int MeasurementCount { get; set; }
        
        // Timing statistics
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double P90 { get; set; }
        public double P95 { get; set; }
        public double P99 { get; set; }
        public double StandardDeviation { get; set; }
        
        // Memory metrics
        public double WorkingSetDeltaMB { get; set; }
        public double PrivateMemoryDeltaMB { get; set; }
        public double PeakWorkingSetMB { get; set; }
        public double PeakPrivateMemoryMB { get; set; }
        
        // GC metrics
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public double TotalGCPauseTimeMs { get; set; }
        
        // CPU metrics
        public double AverageCpuPercent { get; set; }
        public double MaxCpuPercent { get; set; }
        
        // Detailed snapshots
        public List<MemorySnapshot> MemorySnapshots { get; set; } = new();
        public List<CpuSnapshot> CpuSnapshots { get; set; } = new();
    }
}
