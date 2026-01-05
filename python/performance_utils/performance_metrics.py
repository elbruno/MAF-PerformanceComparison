"""
Enhanced performance metrics tracker for Python providing accurate memory, CPU, and statistical measurements.
"""

import gc
import os
import time
from dataclasses import dataclass, field
from typing import List
import psutil
import statistics


@dataclass
class MemorySnapshot:
    """Memory usage snapshot at a point in time."""
    timestamp_ms: float
    rss_mb: float
    vms_mb: float
    shared_mb: float
    available_system_mb: float
    gc_gen0_count: int
    gc_gen1_count: int
    gc_gen2_count: int


@dataclass
class CpuSnapshot:
    """CPU usage snapshot at a point in time."""
    timestamp_ms: float
    cpu_percent: float
    thread_count: int


@dataclass
class MetricsResult:
    """Complete performance metrics result."""
    total_elapsed_ms: float
    measurement_count: int
    
    # Timing statistics
    mean: float = 0.0
    median: float = 0.0
    min: float = 0.0
    max: float = 0.0
    p90: float = 0.0
    p95: float = 0.0
    p99: float = 0.0
    stdev: float = 0.0
    
    # Memory metrics
    rss_delta_mb: float = 0.0
    vms_delta_mb: float = 0.0
    peak_rss_mb: float = 0.0
    peak_vms_mb: float = 0.0
    
    # GC metrics
    gc_gen0_collections: int = 0
    gc_gen1_collections: int = 0
    gc_gen2_collections: int = 0
    
    # CPU metrics
    average_cpu_percent: float = 0.0
    max_cpu_percent: float = 0.0
    
    # Detailed snapshots
    memory_snapshots: List[MemorySnapshot] = field(default_factory=list)
    cpu_snapshots: List[CpuSnapshot] = field(default_factory=list)


class PerformanceMetrics:
    """Enhanced performance metrics tracker."""
    
    def __init__(self):
        self._process = psutil.Process(os.getpid())
        self._start_time = None
        self._measurements: List[float] = []
        self._memory_snapshots: List[MemorySnapshot] = []
        self._cpu_snapshots: List[CpuSnapshot] = []
        
        self._start_rss = 0
        self._start_vms = 0
        self._gc_gen0_start = 0
        self._gc_gen1_start = 0
        self._gc_gen2_start = 0
    
    def start(self):
        """Start performance measurement session."""
        # Force garbage collection to get clean baseline
        gc.collect()
        gc.collect()
        gc.collect()
        
        # Get starting memory
        mem_info = self._process.memory_info()
        self._start_rss = mem_info.rss
        self._start_vms = mem_info.vms
        
        # Get starting GC counts
        gc_counts = gc.get_count()
        self._gc_gen0_start = gc_counts[0]
        self._gc_gen1_start = gc_counts[1]
        self._gc_gen2_start = gc_counts[2]
        
        self._start_time = time.perf_counter()
    
    def record_measurement(self, value_ms: float):
        """Record a single measurement (e.g., one iteration time in milliseconds)."""
        self._measurements.append(value_ms)
    
    def capture_memory_snapshot(self):
        """Capture a memory snapshot at the current point in time."""
        elapsed_ms = (time.perf_counter() - self._start_time) * 1000
        mem_info = self._process.memory_info()
        
        # Get GC stats
        gc_counts = gc.get_count()
        
        # Get system memory
        vm = psutil.virtual_memory()
        
        snapshot = MemorySnapshot(
            timestamp_ms=elapsed_ms,
            rss_mb=mem_info.rss / 1024 / 1024,
            vms_mb=mem_info.vms / 1024 / 1024,
            shared_mb=getattr(mem_info, 'shared', 0) / 1024 / 1024,
            available_system_mb=vm.available / 1024 / 1024,
            gc_gen0_count=gc_counts[0] - self._gc_gen0_start,
            gc_gen1_count=gc_counts[1] - self._gc_gen1_start,
            gc_gen2_count=gc_counts[2] - self._gc_gen2_start
        )
        self._memory_snapshots.append(snapshot)
    
    def capture_cpu_snapshot(self):
        """Capture a CPU usage snapshot."""
        elapsed_ms = (time.perf_counter() - self._start_time) * 1000
        
        # Get CPU percent with a short interval
        cpu_percent = self._process.cpu_percent(interval=0.1)
        thread_count = self._process.num_threads()
        
        snapshot = CpuSnapshot(
            timestamp_ms=elapsed_ms,
            cpu_percent=cpu_percent,
            thread_count=thread_count
        )
        self._cpu_snapshots.append(snapshot)
    
    def get_result(self) -> MetricsResult:
        """Stop measurement and calculate comprehensive metrics."""
        total_elapsed_ms = (time.perf_counter() - self._start_time) * 1000
        
        result = MetricsResult(
            total_elapsed_ms=total_elapsed_ms,
            measurement_count=len(self._measurements)
        )
        
        # Statistical measurements
        if self._measurements:
            sorted_measurements = sorted(self._measurements)
            result.mean = statistics.mean(self._measurements)
            result.min = min(self._measurements)
            result.max = max(self._measurements)
            result.median = statistics.median(self._measurements)
            result.p90 = self._percentile(sorted_measurements, 0.90)
            result.p95 = self._percentile(sorted_measurements, 0.95)
            result.p99 = self._percentile(sorted_measurements, 0.99)
            if len(self._measurements) > 1:
                result.stdev = statistics.stdev(self._measurements)
        
        # Memory metrics
        mem_info = self._process.memory_info()
        result.rss_delta_mb = (mem_info.rss - self._start_rss) / 1024 / 1024
        result.vms_delta_mb = (mem_info.vms - self._start_vms) / 1024 / 1024
        
        # Peak memory (if available)
        try:
            mem_full = self._process.memory_full_info()
            result.peak_rss_mb = getattr(mem_full, 'uss', mem_info.rss) / 1024 / 1024
            result.peak_vms_mb = mem_info.vms / 1024 / 1024
        except:
            result.peak_rss_mb = mem_info.rss / 1024 / 1024
            result.peak_vms_mb = mem_info.vms / 1024 / 1024
        
        # GC metrics
        gc_counts = gc.get_count()
        result.gc_gen0_collections = gc_counts[0] - self._gc_gen0_start
        result.gc_gen1_collections = gc_counts[1] - self._gc_gen1_start
        result.gc_gen2_collections = gc_counts[2] - self._gc_gen2_start
        
        # CPU metrics
        if self._cpu_snapshots:
            result.average_cpu_percent = statistics.mean(s.cpu_percent for s in self._cpu_snapshots)
            result.max_cpu_percent = max(s.cpu_percent for s in self._cpu_snapshots)
        
        # Detailed snapshots
        result.memory_snapshots = self._memory_snapshots
        result.cpu_snapshots = self._cpu_snapshots
        
        return result
    
    @staticmethod
    def _percentile(sorted_values: List[float], percentile: float) -> float:
        """Calculate percentile from sorted values."""
        if not sorted_values:
            return 0.0
        
        index = percentile * (len(sorted_values) - 1)
        lower = int(index)
        upper = int(index + 0.5) if index != lower else lower
        
        if lower == upper or upper >= len(sorted_values):
            return sorted_values[lower]
        
        fraction = index - lower
        return sorted_values[lower] * (1 - fraction) + sorted_values[upper] * fraction
