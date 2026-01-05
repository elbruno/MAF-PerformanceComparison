# Scenario 3: Web Frontend with Enhanced Metrics (Aspire)

## Overview

This scenario provides a **real-time web application** for interactive performance testing using **.NET Aspire** orchestration. It combines the enhanced metrics from [Scenario 2](../scenario-2-enhanced-metrics/README.md) with a modern web interface for real-time monitoring and comparison.

> **Note**: This is the same test functionality as Scenarios 1 & 2, but with a web frontend and real-time monitoring capabilities.

For full technical details, see [README-original.md](README-original.md).

## Quick Start

```bash
cd AppHost/PerformanceComparison.AppHost
dotnet run
```

Then open the Aspire dashboard and navigate to the Web Frontend to see **real-time enhanced metrics**!

## What Makes This Different

| Feature | Scenarios 1 & 2 | Scenario 3 |
|---------|----------------|-----------|
| Interface | Command-line | **Web UI** |
| Metrics | Basic (S1) / Enhanced (S2) | **Enhanced (S2)** |
| Real-time updates | No | **Yes** ✅ |
| Orchestration | Manual | **Aspire** ✅ |
| Monitoring | Logs only | **Dashboard** ✅ |

## Enhanced Metrics Displayed

Same comprehensive metrics as Scenario 2:
- **Statistics**: Mean, Median, P90, P95, P99, StdDev
- **Memory**: Working Set, Private Memory, Peak
- **GC**: Gen0/1/2 counts, Pause time
- **CPU**: Average and Max percentage
- **Trends**: Memory and CPU snapshots

## Documentation

- [Full README (v1.0)](README-original.md) - Original Aspire documentation
- [Architecture](ARCHITECTURE.md) - Technical architecture
- [Testing Guide](TESTING_GUIDE.md) - Testing instructions
- [Scenario 2](../scenario-2-enhanced-metrics/README.md) - Enhanced metrics reference
- [Main README](../README.md) - Project overview
