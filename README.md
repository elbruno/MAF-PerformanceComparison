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

**PowerShell (Windows):**

```powershell
.\run_tests.ps1
```

**Bash (Linux/Mac):**

```bash
./run_tests.sh
```

**Command Prompt (Windows):**

```cmd
run_tests.bat
```

These scripts will:

- Run performance tests for both .NET and Python implementations
- Use Ollama by default (10 iterations)
- Export metrics to JSON files for analysis

### Customize Test Parameters

```powershell
# Run with 1000 iterations in different test modes
.\run_tests.ps1 -Iterations 1000 -TestMode standard
.\run_tests.ps1 -Iterations 1000 -TestMode batch
.\run_tests.ps1 -Iterations 1000 -TestMode concurrent
.\run_tests.ps1 -Iterations 1000 -TestMode streaming
.\run_tests.ps1 -Iterations 1000 -TestMode scenarios
```

**Bash:**

```bash
# Run with 1000 iterations in different test modes
ITERATIONS=1000 TEST_MODE=standard ./run_tests.sh
ITERATIONS=1000 TEST_MODE=batch ./run_tests.sh
ITERATIONS=1000 TEST_MODE=concurrent ./run_tests.sh
ITERATIONS=1000 TEST_MODE=streaming ./run_tests.sh
ITERATIONS=1000 TEST_MODE=scenarios ./run_tests.sh
```

## Test Modes

- **standard**: Sequential execution (baseline)
- **batch**: Batch processing with configurable batch size
- **concurrent**: Multiple concurrent requests
- **streaming**: Streaming responses with time-to-first-token metrics
- **scenarios**: Multiple prompt types (simple, reasoning, conceptual)

## Organize and Analyze Results

After running tests, organize the results:

```bash
python organize_results.py
```

This will:

- Create timestamped folders in `tests_results/`
- Move all metrics files to organized folders
- Generate comparison reports
- Automatically analyze results using available LLMs

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
