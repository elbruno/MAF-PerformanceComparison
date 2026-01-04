# Test Automation Scripts

The Microsoft Agent Framework performance testing suite is automated using a unified Python script that orchestrates both test execution and results analysis.

## Unified Test Runner

The `run_performance_tests.py` script is a cross-platform Python application that:

1. **Executes Tests**: Runs both .NET and Python Agent Framework implementations
2. **Manages Environment**: Sets up test configuration and variables
3. **Organizes Results**: Collects metrics into timestamped folders
4. **Generates Reports**: Creates comparison markdown files
5. **Analyzes Results**: Uses Ollama to generate AI-powered insights

## Usage Examples

### Basic Usage

Run tests with default settings (Ollama agent, standard mode, 10 iterations):

```bash
python run_performance_tests.py
```

### Customize Test Mode

Specify different test modes using the `-m` or `--test-mode` flag:

```bash
# Standard mode (default) - sequential execution
python run_performance_tests.py -m standard

# Batch processing mode
python run_performance_tests.py -m batch -b 20

# Concurrent requests mode
python run_performance_tests.py -m concurrent -c 10

# Streaming mode
python run_performance_tests.py -m streaming

# Scenarios mode (tests multiple prompt types)
python run_performance_tests.py -m scenarios
```

### Select Agent Type

Use the `-a` or `--agent-type` flag to choose which agent to test:

```bash
# Test HelloWorld agent only
python run_performance_tests.py -a HelloWorld

# Test AzureOpenAI agent only (requires .env configuration)
python run_performance_tests.py -a AzureOpenAI

# Test Ollama agent only (requires Ollama installation)
python run_performance_tests.py -a Ollama

# Test all available agents
python run_performance_tests.py -a All
```

### Customize Iteration Count

```bash
# Run 100 iterations
python run_performance_tests.py -i 100

# Run 1000 iterations for more statistical significance
python run_performance_tests.py -i 1000

# Run with custom batch size (for batch mode)
python run_performance_tests.py -m batch -i 500 -b 25
```

### Combined Examples

Run all agents in batch mode with 100 iterations and batch size of 15:

```bash
python run_performance_tests.py -a All -m batch -i 100 -b 15
```

Run Ollama agent in concurrent mode with 50 concurrent requests and 200 iterations:

```bash
python run_performance_tests.py -a Ollama -m concurrent -i 200 -c 50
```

Test HelloWorld in streaming mode and skip automatic analysis:

```bash
python run_performance_tests.py -a HelloWorld -m streaming --skip-analysis
```

## Organizing and Analyzing Results

The unified test runner automatically organizes and analyzes results after each test run. If you want to process existing metrics files without running tests:

```bash
# Process results only (without running tests)
python run_performance_tests.py --process-only
```

### What Gets Generated

- **Timestamped Folder**: `tests_results/YYYYMMDD_HHMMSS_{test_mode}_{iterations}iter/`
- **Comparison Report**: `comparison_report_{test_mode}_{iterations}iter.md` - Detailed metrics with LLM-ready prompts
- **Analysis Report**: `analysis_report_{test_mode}_{iterations}iter.md` - AI-powered analysis from Ollama

### Configuration

Set environment variables in a `.env` file:

```bash
# For Ollama
OLLAMA_ENDPOINT=http://localhost:11434
OLLAMA_MODEL_NAME=ministral-3
```

## Workflow Example

Complete workflow for running tests and analyzing results:

```bash
# 1. Run tests in standard mode with 1000 iterations for HelloWorld agent
python run_performance_tests.py -a HelloWorld -m standard -i 1000

# 2. Results are automatically organized and analyzed
# Results will be in: tests_results/YYYYMMDD_HHMMSS_standard_1000iter/

# 3. Or, process existing metrics without running tests
python run_performance_tests.py --process-only
```

## Test Modes Reference

| Mode | Description | Parameters |
|------|-------------|------------|
| `standard` | Sequential execution | - |
| `batch` | Batch processing | `-b, --batch-size` |
| `concurrent` | Concurrent requests | `-c, --concurrent-requests` |
| `streaming` | Streaming responses with TTFT | - |
| `scenarios` | Multiple prompt types | - |

## Agent Types Reference

| Agent Type | Description | Requirements |
|------------|-------------|--------------|
| `HelloWorld` | Demo/mock agent | None |
| `AzureOpenAI` | Azure OpenAI integration | `.env` file with credentials |
| `Ollama` | Local Ollama integration | Ollama installed and running |
| `All` | Run all available agents | Optional for AzureOpenAI and Ollama |

## All Available Options

View all available command-line options:

```bash
python run_performance_tests.py --help
```

Key options:

- `-a, --agent-type`: Agent type to test (default: Ollama)
- `-m, --test-mode`: Test execution mode (default: standard)
- `-i, --iterations`: Number of test iterations (default: 10)
- `-b, --batch-size`: Batch size for batch mode (default: 10)
- `-c, --concurrent-requests`: Concurrent requests (default: 5)
- `--process-only`: Process results without running tests
- `--skip-cleanup`: Skip cleaning old metrics files
- `--skip-analysis`: Skip Ollama analysis in results

## Output Files

### Metrics Files

Format: `metrics_{language}_{provider}_{timestamp}.json`

Examples:

- `metrics_dotnet_ollama_20260103_123456.json`
- `metrics_python_ollama_20260103_123456.json`

### Organized Results

After running tests, results are in:

```
tests_results/
└── 20260103_123456_standard_1000iter/
    ├── metrics_dotnet_ollama_20260103_123456.json
    ├── metrics_python_ollama_20260103_123456.json
    ├── comparison_report_standard_1000iter.md
    └── analysis_report_standard_1000iter.md
```

## Troubleshooting

### Python Dependencies

If tests fail with import errors, install dependencies:

```bash
pip install -r requirements.txt
```

### .NET Build Issues

If .NET tests fail to build:

```bash
# Restore NuGet packages
cd dotnet/OllamaAgent
dotnet restore
dotnet build
```

### Ollama Not Reachable

If analysis fails with connection errors:

1. Verify Ollama is running: `ollama serve`
2. Check endpoint: `curl http://localhost:11434/api/tags`
3. Set custom endpoint: Use `OLLAMA_ENDPOINT` environment variable
