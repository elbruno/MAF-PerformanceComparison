# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework implementations across Python and .NET. The framework provides a unified approach to building AI agents with advanced capabilities for orchestration, tool calling, and multi-agent workflows.

## 🆕 New: Enhanced Performance Metrics

We've significantly improved the accuracy and detail of performance measurements!

**Key Improvements:**

- 📊 **Statistical Rigor**: P90, P95, P99 percentiles, median, standard deviation
- 💾 **Accurate Memory Tracking**: Process-level metrics (Working Set, Private Memory, Peak usage)
- ⚡ **Precise CPU Monitoring**: Time-based calculations with proper multi-core accounting
- 🔍 **GC Insights**: Per-generation collection counts and pause time tracking
- 📈 **Trend Analysis**: Memory and CPU snapshots over time

**Quick Start:** Run tests as usual - enhanced metrics are collected automatically!

```bash
python run_performance_tests.py -i 1000
```

**Learn more**: See [Performance Quick Start](docs/PERFORMANCE_QUICK_START.md) or [Detailed Guide](docs/PERFORMANCE_IMPROVEMENTS.md)

---

## 🆕 New: Real-time Web Application with Aspire

We've added a modern web application for interactive performance testing!

**Features:**

- 🚀 Real-time performance comparison between .NET and Python
- 🎛️ Configure test parameters through a web UI
- 📊 Live results and side-by-side comparisons
- 🔍 Aspire dashboard for unified monitoring and telemetry

**Quick Start:**

```bash
cd aspire-web/AppHost/PerformanceComparison.AppHost
dotnet run
```

**Learn more**: See `docs/aspire-web-README.md` and `docs/ASPIRE_WEB.md` for the Aspire web scenario documentation

Quick mode: run the Aspire-based web dashboard to interactively run tests and view live progress:

```bash
cd aspire-web/AppHost/PerformanceComparison.AppHost
dotnet run
```

Then open the Aspire dashboard and navigate to `/dashboard` for the Performance Dashboard UI.

---

## Quick Start with Ollama

The fastest way to get started is using Ollama with local models:

### Prerequisites

1. Install [Ollama](https://ollama.ai/)
2. Pull a model: `ollama pull ministral-3`
3. Install [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
4. Install [Python 3.10+](https://www.python.org/downloads/)

### Run Tests

Use the unified Python test runner to execute tests and generate reports:

```python
python run_performance_tests.py
```

This will:

- Run performance tests for both .NET and Python implementations
- Use Ollama by default (10 iterations in standard mode)
- Export metrics to JSON files
- Automatically organize results and generate comparison reports
- Generate AI-powered analysis using Ollama

### Customize Test Parameters

```bash
# Run with 100 iterations in standard mode
python run_performance_tests.py -i 100

# Run all agent types with 1000 iterations
python run_performance_tests.py -a All -i 1000

# Run tests in batch mode with custom batch size
python run_performance_tests.py -m batch -b 20

# Run tests in concurrent mode with 10 parallel requests
python run_performance_tests.py -m concurrent -c 10

# Use a specific model for both .NET and Python agents (default: ministral-3)
python run_performance_tests.py --model mistral

# Test different modes
python run_performance_tests.py -m streaming
python run_performance_tests.py -m scenarios

# Process results only (without running tests)
python run_performance_tests.py --process-only

# Run tests but skip Ollama analysis
python run_performance_tests.py --skip-analysis
```

**View all options:**

```bash
python run_performance_tests.py --help
```

## Test Modes

- **standard**: Sequential execution (baseline)
- **batch**: Batch processing with configurable batch size
- **concurrent**: Multiple concurrent requests
- **streaming**: Streaming responses with time-to-first-token metrics
- **scenarios**: Multiple prompt types (simple, reasoning, conceptual)

## Results Organization and Analysis

The unified test runner automatically organizes and analyzes results after each test run. Results are:

- **Organized** into timestamped folders in `tests_results/`
- **Compared** using comparison reports with LLM prompts
- **Analyzed** with AI-powered insights from your Ollama model

You can also manually process existing metrics files:

```bash
python run_performance_tests.py --process-only
```

## What Gets Measured

### Enhanced Metrics (New!)
- **Timing Statistics**: Mean, median, min, max, P90, P95, P99, standard deviation
- **Process Memory**: Working Set, Private Memory, Peak usage, memory trends
- **CPU Monitoring**: Time-based accurate CPU %, thread count, usage patterns
- **Garbage Collection**: Per-generation counts (Gen0/1/2), GC pause time
- **Trend Analysis**: Memory and CPU snapshots over time

### Legacy Metrics
- **Execution Time**: Total time, average per iteration
- **Advanced Metrics**: Time-to-first-token, batch throughput, concurrent latency

For details, see [Performance Improvements Guide](docs/PERFORMANCE_IMPROVEMENTS.md)

## Project Features

- ✅ **Enhanced Performance Metrics**: Industry-standard measurements with statistical rigor
- ✅ **Multiple AI Providers**: Azure OpenAI, Ollama, and demo modes
- ✅ **Cross-platform**: Windows, Linux, macOS
- ✅ **Multiple Test Modes**: Standard, batch, concurrent, streaming, scenarios
- ✅ **Comprehensive Metrics**: Time, memory, CPU, GC, statistical analysis
- ✅ **Automated Testing**: Scripts for easy test execution
- ✅ **LLM-Ready Analysis**: JSON export for AI-powered comparisons

## Learn More

### Performance Testing
- **[Performance Quick Start](docs/PERFORMANCE_QUICK_START.md)** - What's new and how to use enhanced metrics
- **[Performance Improvements](docs/PERFORMANCE_IMPROVEMENTS.md)** - Detailed technical guide
- **[Performance Recommendations](docs/PERFORMANCE_RECOMMENDATIONS.md)** - Complete analysis and roadmap
- **[Migration Guide](docs/MIGRATION_GUIDE.md)** - Apply improvements to other agents

### General Documentation
- [Detailed Guide](docs/DETAILED_GUIDE.md) - Comprehensive documentation
- [Scripts Documentation](docs/SCRIPTS_README.md) - Test automation details
- [Comparison Template](docs/comparison_prompt_template.md) - LLM analysis prompts

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Agent Framework Python GitHub](https://github.com/microsoft/agent-framework/tree/main/python)
- [Agent Framework .NET GitHub](https://github.com/microsoft/agent-framework/tree/main/dotnet)
- [Ollama](https://ollama.ai/)
