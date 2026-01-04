# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework implementations across Python and .NET. The framework provides a unified approach to building AI agents with advanced capabilities for orchestration, tool calling, and multi-agent workflows.

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

- **Execution Time**: Total time, average per iteration, min/max
- **Memory Usage**: RAM consumed during execution
- **CPU Utilization**: Processor usage across platforms
- **Statistical Analysis**: Median, standard deviation
- **Advanced Metrics**: Time-to-first-token, batch throughput, concurrent latency

## Project Features

- ✅ **Multiple AI Providers**: Azure OpenAI, Ollama, and demo modes
- ✅ **Cross-platform**: Windows, Linux, macOS
- ✅ **Multiple Test Modes**: Standard, batch, concurrent, streaming, scenarios
- ✅ **Comprehensive Metrics**: Time, memory, CPU, statistical analysis
- ✅ **Automated Testing**: Scripts for easy test execution
- ✅ **LLM-Ready Analysis**: JSON export for AI-powered comparisons

## Learn More

For detailed information, advanced configuration, and troubleshooting:

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
