# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework implementations across Python and .NET. The framework provides a unified approach to building AI agents with advanced capabilities for orchestration, tool calling, and multi-agent workflows.

## 🎯 Three Testing Scenarios

This project is organized into **three distinct scenarios** to demonstrate different approaches to performance testing:

### [Scenario 1: Basic Metrics (Learning Phase)](scenario-1-basic-metrics/)

**Original/basic implementation** showing common but insufficient approaches:

- Basic timing with stopwatch
- Simple memory tracking (GC heap only)
- Instantaneous CPU samples
- Mean, min, max statistics only

**Use for**: Learning what NOT to do, understanding limitations

### [Scenario 2: Enhanced Metrics (Production-Ready)](scenario-2-enhanced-metrics/) ⭐ **Recommended**

**Industry-standard implementation** with comprehensive metrics:

- 📊 **Statistical Rigor**: P90, P95, P99 percentiles, median, standard deviation
- 💾 **Accurate Memory**: Process-level metrics (Working Set, Private Memory, Peak)
- ⚡ **Precise CPU**: Time-based calculations with multi-core accounting
- 🔍 **GC Insights**: Per-generation counts and pause time tracking
- 📈 **Trend Analysis**: Memory and CPU snapshots over time

**Use for**: Production testing, accurate comparisons, CI/CD

### [Scenario 3: Web Frontend (Aspire)](scenario-3-aspire-web/)

**Real-time web application** with Scenario 2 metrics + modern UI:

- 🚀 Real-time comparison with live updates
- 🎛️ Interactive configuration through web UI
- 📊 Visual dashboards with enhanced metrics
- 🔍 Aspire orchestration and monitoring

**Use for**: Interactive testing, demonstrations, real-time monitoring

---

## Quick Start

### Scenario 1: Basic Metrics (Learning)

```bash
# Using test runner (recommended)
cd scenario-1-basic-metrics
python run_tests.py -i 100 -a Ollama

# Or manually
cd scenario-1-basic-metrics/dotnet/OllamaAgent
dotnet build && ITERATIONS=100 dotnet run
```

### Scenario 2: Enhanced Metrics (Recommended)

```bash
# Using test runner (recommended)
cd scenario-2-enhanced-metrics
python run_tests.py -i 1000 -a Ollama

# Or manually
cd scenario-2-enhanced-metrics/dotnet/OllamaAgent
dotnet build && ITERATIONS=1000 dotnet run
```

### Scenario 3: Web Frontend

```bash
aspire run
```

**Note**: Scenarios 1 and 2 include a `run_tests.py` script that runs both .NET and Python tests automatically and processes results. See each scenario's README for details.

---

## Scenario Comparison

| Feature | Scenario 1 | Scenario 2 | Scenario 3 |
|---------|-----------|-----------|-----------|
| **Metrics** | Basic | **Enhanced** | **Enhanced** |
| **Interface** | CLI | CLI | **Web UI** |
| **Real-time** | ❌ | ❌ | ✅ |
| **P90/P95/P99** | ❌ | ✅ | ✅ |
| **GC Insights** | ❌ | ✅ | ✅ |
| **Orchestration** | Manual | Manual | **Aspire** |
| **Best for** | Learning | Production | Demos |

---

## What's Measured (Scenarios 2 & 3)

### Enhanced Metrics

- **Timing**: Mean, Median, P90, P95, P99, StdDev, Min, Max
- **Memory**: Working Set, Private Memory, Peak, Trends
- **CPU**: Time-based accurate %, Thread count, Patterns
- **GC**: Gen0/1/2 counts, Pause time

### Example Output

```
Timing: Mean: 125ms  Median: 123ms  P95: 168ms  P99: 235ms
Memory: Working Set: 46MB    Peak: 257MB
GC: Gen0/1/2: 234/45/3      Pause: 457ms
CPU: Average: 23%           Max: 46%
```

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- [Python 3.10+](https://www.python.org/downloads/)
- [Ollama](https://ollama.ai/): `ollama pull ministral-3`
- **Scenario 3**: `dotnet workload install aspire`

---

## Project Structure

```
MAF-PerformanceComparison/
├── scenario-1-basic-metrics/      # Basic (learning)
├── scenario-2-enhanced-metrics/   # Production (recommended)
├── scenario-3-aspire-web/         # Web frontend
├── docs/                          # Documentation
│   ├── PERFORMANCE_QUICK_START.md
│   ├── PERFORMANCE_IMPROVEMENTS.md
│   ├── MIGRATION_GUIDE.md
│   └── PERFORMANCE_RECOMMENDATIONS.md
└── README.md                      # This file
```

---

## Documentation

### By Scenario

- **[Scenario 1 README](scenario-1-basic-metrics/README.md)** - Basic metrics
- **[Scenario 2 README](scenario-2-enhanced-metrics/README.md)** - Enhanced metrics
- **[Scenario 3 README](scenario-3-aspire-web/README.md)** - Web frontend

### Performance Testing

- **[Performance Quick Start](docs/PERFORMANCE_QUICK_START.md)** - Getting started
- **[Performance Improvements](docs/PERFORMANCE_IMPROVEMENTS.md)** - Technical details
- **[Migration Guide](docs/MIGRATION_GUIDE.md)** - Apply to other agents
- **[Performance Recommendations](docs/PERFORMANCE_RECOMMENDATIONS.md)** - Roadmap
- **[Executive Summary](docs/EXECUTIVE_SUMMARY.md)** - Overview

---

## Which Scenario Should I Use?

### 🎓 For Learning

**Scenario 1**: Understand common mistakes and limitations

### 🏭 For Production

**Scenario 2**: Accurate metrics, CI/CD, statistical rigor

### 🎨 For Demos

**Scenario 3**: Real-time monitoring, visual dashboards

---

## Key Metrics Explained

- **P95/P99**: 95%/99% of requests complete within this time (SLA planning)
- **StdDev**: Low = consistent, High = investigate variability
- **Gen2 Collections**: Should be rare (< 10 for 1000 iterations)
- **GC Pause**: Should be < 5% of total time
- **Working Set**: Actual physical RAM used
- **Peak Memory**: Maximum resource requirement

---

## Features

- ✅ **Three Progressive Scenarios**: Learning → Production → Web
- ✅ **Enhanced Metrics**: Industry-standard (S2 & S3)
- ✅ **Multiple Providers**: Azure OpenAI, Ollama
- ✅ **Cross-platform**: Windows, Linux, macOS
- ✅ **Comprehensive Docs**: 60KB+ guides
- ✅ **Web Dashboard**: Real-time with Aspire (S3)

---

## License

MIT License - see [LICENSE](LICENSE) file

---

## Resources

- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/)
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Ollama](https://ollama.ai/)
