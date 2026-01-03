# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework implementations across different programming languages and AI service providers. The framework provides a unified approach to building AI agents with advanced capabilities for orchestration, tool calling, and multi-agent workflows.

## Project Structure

```
├── dotnet/
│   ├── HelloWorldAgent/           # Basic C# agent (demo/mock mode)
│   │   ├── Program.cs
│   │   └── HelloWorldAgent.csproj
│   ├── AzureOpenAIAgent/          # C# agent with Azure OpenAI
│   │   ├── Program.cs
│   │   ├── AzureOpenAIAgent.csproj
│   │   └── .env.example
│   └── OllamaAgent/               # C# agent with Ollama
│       ├── Program.cs
│       ├── OllamaAgent.csproj
│       └── .env.example
└── python/
    ├── hello_world_agent/         # Basic Python agent (demo/mock mode)
    │   ├── main.py
    │   └── requirements.txt
    ├── azure_openai_agent/        # Python agent with Azure OpenAI
    │   ├── main.py
    │   ├── requirements.txt
    │   └── .env.example
    └── ollama_agent/              # Python agent with Ollama
        ├── main.py
        ├── requirements.txt
        └── .env.example
```

## Prerequisites

### For .NET (C#)

- .NET 10.0 SDK or later
- Microsoft.Agents.AI NuGet package (automatically installed)
- Azure.Identity NuGet package (for authentication)

### For Python

- Python 3.10 or later
- agent-framework or agent-framework-core package
- psutil package (for performance measurement)
- python-dotenv package (for configuration)
- azure-ai-inference package (for Azure OpenAI integration)

### For Azure OpenAI Integration

- Azure OpenAI Service resource
- API endpoint and key
- Deployed model (e.g., gpt-5-mini)

### For Ollama Integration

- Ollama installed locally ([Download here](https://ollama.ai/))
- At least one model pulled (e.g., `ollama pull ministral-3`)

## Getting Started

### 1. Basic Hello World Agents (Demo Mode)

These agents demonstrate the framework structure without requiring external services.

**C# Version:**

```bash
cd dotnet/HelloWorldAgent
dotnet build
dotnet run
```

**Python Version:**

```bash
cd python/hello_world_agent
pip install -r requirements.txt
python main.py
```

### 2. Azure OpenAI Agents

**Setup:**

1. Copy `.env.example` to `.env` in the respective agent directory
2. Fill in your Azure OpenAI credentials:

   ```
   AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
   AZURE_OPENAI_DEPLOYMENT_NAME=gpt-5-mini
   ```

3. Authenticate with Azure CLI: `az login`

**C# Version:**

```bash
cd dotnet/AzureOpenAIAgent
# Create .env file with your credentials
dotnet build
dotnet run
```

**Python Version:**

```bash
cd python/azure_openai_agent
# Create .env file with your credentials
pip install -r requirements.txt
python main.py
```

### 3. Ollama Agents (Local Models)

**Setup:**

1. Install Ollama from [https://ollama.ai/](https://ollama.ai/)
2. Start Ollama service
3. Pull a model: `ollama pull ministral-3`
4. (Optional) Copy `.env.example` to `.env` to customize endpoint or model

**C# Version:**

```bash
cd dotnet/OllamaAgent
dotnet build
dotnet run
```

**Python Version:**

```bash
cd python/ollama_agent
pip install -r requirements.txt
python main.py
```

## Performance Metrics

Each application automatically runs **1000 iterations** of agent operations to measure performance accurately. 

### Warmup Phase

Before the performance test begins, each agent performs a **warmup call** to ensure the model is loaded and ready. This helps provide more consistent and accurate performance measurements by eliminating first-call overhead.

### Collected Metrics

The following metrics are collected and reported:

- **Total Iterations**: Number of agent operations performed (1000)
- **Total Execution Time**: Time from initialization to completion (milliseconds)
- **Average Time per Iteration**: Mean execution time per agent operation (milliseconds)
- **Min Iteration Time**: Fastest iteration time (milliseconds)
- **Max Iteration Time**: Slowest iteration time (milliseconds)
- **Memory Usage**: RAM consumed during execution (MB)
- **Warmup Status**: Whether the warmup call was successful

Example output:

```
⏳ Performing warmup call to prepare the model...
✓ Warmup completed in 1234.567 ms
✓ Running 1000 iterations for performance testing

=== Performance Metrics ===
Total Iterations: 1000
Total Execution Time: 1245 ms
Average Time per Iteration: 1.234 ms
Min Iteration Time: 0.985 ms
Max Iteration Time: 2.156 ms
Memory Used: 3.45 MB
========================
✓ Metrics exported to: metrics_dotnet_ollama_20260103_123456.json
```

### Metrics Export

After each test run, performance metrics are automatically exported to a JSON file for easy comparison and analysis. The files are named using the pattern:

- .NET Ollama: `metrics_dotnet_ollama_[timestamp].json`
- Python Ollama: `metrics_python_ollama_[timestamp].json`
- .NET Azure OpenAI: `metrics_dotnet_azureopenai_[timestamp].json`
- Python Azure OpenAI: `metrics_python_azureopenai_[timestamp].json`

The JSON format is structured to be easily understood by LLMs for automated comparison and analysis.

Example metrics file structure:

```json
{
  "TestInfo": {
    "Language": "CSharp",
    "Framework": "DotNet",
    "Provider": "Ollama",
    "Model": "ministral-3",
    "Endpoint": "http://localhost:11434",
    "Timestamp": "2026-01-03T12:34:56.789Z",
    "WarmupSuccessful": true
  },
  "Metrics": {
    "TotalIterations": 1000,
    "TotalExecutionTimeMs": 1245,
    "AverageTimePerIterationMs": 1.234,
    "MinIterationTimeMs": 0.985,
    "MaxIterationTimeMs": 2.156,
    "MemoryUsedMB": 3.45
  }
}
```

## Configuration

### Azure OpenAI Configuration

Set these environment variables or create a `.env` file:

| Variable | Description | Example |
|----------|-------------|---------|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI service endpoint | `https://your-resource.openai.azure.com/` |
| `AZURE_OPENAI_DEPLOYMENT_NAME` | Deployed model name | `gpt-5-mini` |

**Note:** Authentication is handled via Azure CLI. Run `az login` before using Azure OpenAI agents.

### Ollama Configuration

Set these environment variables or create a `.env` file (optional - defaults provided):

| Variable | Description | Default |
|----------|-------------|---------|
| `OLLAMA_ENDPOINT` | Ollama service endpoint | `http://localhost:11434` |
| `OLLAMA_MODEL_NAME` | Model to use | `ministral-3` |

## Features

- ✅ **Basic Hello World Agents**: Simple implementations in C# and Python with 1000-iteration performance testing
- ✅ **Azure OpenAI Integration**: Cloud-based AI service integration
- ✅ **Ollama Integration**: Local model support for privacy and offline use
- ✅ **Comprehensive Performance Metrics**: Built-in time and memory tracking with statistical analysis
- ✅ **Model Warmup**: Automatic warmup calls to ensure consistent performance measurements
- ✅ **Metrics Export**: Automatic JSON export of performance data for easy comparison
- ✅ **Cross-platform**: Works on Windows, Linux, and macOS
- ✅ **Configuration via Environment Variables**: Easy setup with `.env` files
- ✅ **1000-Iteration Testing**: Statistically significant performance measurements
- ✅ **LLM-Ready Comparison**: AI-friendly format for automated performance analysis

## Comparing Performance

### Automated Comparison with LLMs

The project includes a comprehensive comparison prompt template that enables easy performance analysis using Large Language Models.

**Quick Start:**

1. **Run performance tests** for the implementations you want to compare:
   ```bash
   # Example: Compare .NET vs Python for Ollama
   cd dotnet/OllamaAgent && dotnet run
   cd ../../python/ollama_agent && python main.py
   ```

2. **Locate the generated metrics files** (automatically created after each run):
   - `metrics_dotnet_ollama_[timestamp].json`
   - `metrics_python_ollama_[timestamp].json`

3. **Use the comparison prompt** from `comparison_prompt_template.md`:
   - Open the template file
   - Copy the prompt
   - Paste the content of your two metrics JSON files into the prompt
   - Submit to your preferred LLM (ChatGPT, Claude, Copilot, etc.)

4. **Get comprehensive analysis** including:
   - Performance comparison (speed, consistency, efficiency)
   - Resource usage analysis
   - Statistical insights
   - Recommendations

See [comparison_prompt_template.md](comparison_prompt_template.md) for detailed instructions and the full prompt template.

### Manual Comparison

To manually compare performance between implementations:

1. **Run the same scenario** in both C# and Python
2. **Compare the exported JSON files** or terminal output, focusing on:
   - Total execution time for 1000 iterations
   - Average time per iteration
   - Memory usage patterns
   - Performance consistency (min/max range)
3. **Consider factors** like:
   - Warmup success (first call overhead)
   - Model complexity
   - Network latency (for Azure OpenAI)
   - Hardware specifications
   - Language runtime differences (JIT compilation, GC behavior, etc.)

The 1000-iteration approach with warmup provides statistically significant results by averaging out transient performance fluctuations.

## Architecture

Both C# and Python implementations use:

- **Microsoft Agent Framework**: A modern framework for building production-ready AI agents
- **Unified Agent Model**: Consistent patterns across languages for reasoning, memory, and tool integration
- **Modular Design**: Easy to add new scenarios or providers
- **Performance Monitoring**: Built-in metrics collection with 1000-iteration testing
- **Error Handling**: Graceful degradation when services are unavailable
- **Provider Agnostic**: Support for Azure OpenAI, OpenAI, Ollama, and other providers

## Troubleshooting

### Azure OpenAI Agents

**Problem**: "Configuration not found. Using demo mode."

- **Solution**: Create a `.env` file with your Azure OpenAI credentials

**Problem**: Connection errors or authentication failures

- **Solution**:
  - Verify your endpoint URL is correct
  - Run `az login` to authenticate with Azure CLI
  - Ensure your Azure account has access to the Azure OpenAI resource

### Ollama Agents

**Problem**: "Could not connect to Ollama"

- **Solution**: Ensure Ollama is running (`ollama serve`) and the model is downloaded

**Problem**: "Model not found"

- **Solution**: Pull the model first: `ollama pull ministral-3`

## Advanced Features

### Test Modes

All agents now support multiple test modes for comprehensive performance analysis:

#### Environment Variables

Configure test behavior using environment variables:

| Variable | Description | Default | Values |
|----------|-------------|---------|--------|
| `TEST_MODE` | Test execution mode | `standard` | `standard`, `batch`, `concurrent`, `streaming`, `scenarios` |
| `ITERATIONS` | Number of test iterations | `1000` | Any positive integer |
| `BATCH_SIZE` | Batch size for batch mode | `10` | Any positive integer |
| `CONCURRENT_REQUESTS` | Concurrent requests count | `5` | Any positive integer |

#### Test Mode Descriptions

1. **Standard Mode** (`TEST_MODE=standard`)
   - Sequential execution of requests
   - Baseline performance measurement
   - Default behavior

2. **Batch Processing Mode** (`TEST_MODE=batch`)
   - Processes requests in batches
   - Measures batch throughput
   - Configurable batch size via `BATCH_SIZE`
   - Example: `TEST_MODE=batch BATCH_SIZE=20 ITERATIONS=100 dotnet run`

3. **Concurrent Mode** (`TEST_MODE=concurrent`)
   - Multiple requests executed concurrently
   - Uses `Task.WhenAll` (.NET) and `asyncio.gather` (Python)
   - Configurable concurrency via `CONCURRENT_REQUESTS`
   - Example: `TEST_MODE=concurrent CONCURRENT_REQUESTS=10 ITERATIONS=50 python main.py`

4. **Streaming Mode** (`TEST_MODE=streaming`)
   - Measures streaming response performance
   - Includes time-to-first-token (TTFT) metrics
   - Demonstrates real-time response capabilities
   - Example: `TEST_MODE=streaming ITERATIONS=100 dotnet run`

5. **Scenarios Mode** (`TEST_MODE=scenarios`)
   - Tests multiple prompt types:
     - Simple: Basic greetings
     - Medium: Conceptual questions
     - Long output: Detailed explanations
     - Reasoning: Logic problems
     - Conceptual: Technical comparisons
   - Runs 200 iterations per scenario
   - Provides comparative analysis across prompt types
   - Example: `TEST_MODE=scenarios python main.py`

### Enhanced Metrics

All agents now export comprehensive metrics including:

#### Core Metrics
- Total iterations completed
- Total execution time
- Average, min, max, and median latency
- Standard deviation
- Memory usage (MB)
- **CPU utilization (%)** - Cross-platform monitoring

#### Advanced Metrics
- **Time-to-first-token (TTFT)** - In streaming mode
- **Scenario-specific performance** - In scenarios mode
- **Batch throughput** - In batch mode
- **Concurrent request latency** - In concurrent mode

#### Metrics Export Format

Enhanced JSON structure with comprehensive data:

```json
{
  "TestInfo": {
    "Language": "CSharp|Python",
    "Framework": "DotNet|Python",
    "Provider": "AzureOpenAI|Ollama|HelloWorld",
    "Model": "gpt-5-mini",
    "TestMode": "standard|batch|concurrent|streaming|scenarios",
    "Timestamp": "2026-01-03T12:34:56.789Z",
    "WarmupSuccessful": true
  },
  "Configuration": {
    "BatchSize": 10,
    "ConcurrentRequests": 5
  },
  "Metrics": {
    "TotalIterations": 1000,
    "TotalExecutionTimeMs": 12345,
    "AverageTimePerIterationMs": 12.345,
    "MinIterationTimeMs": 8.123,
    "MaxIterationTimeMs": 45.678,
    "MedianIterationTimeMs": 11.234,
    "StandardDeviationMs": 3.456,
    "MemoryUsedMB": 45.67,
    "AverageCpuUsagePercent": 23.45,
    "TimeToFirstTokenMs": 156.78,
    "ScenarioResults": {
      "simple": {
        "AverageMs": 10.5,
        "MinMs": 8.2,
        "MaxMs": 15.3,
        "MedianMs": 10.1
      }
    }
  },
  "Summary": "Test completed in standard mode. Processed 1000 iterations..."
}
```

### Example Usage

#### Run Standard Performance Test
```bash
# .NET
cd dotnet/AzureOpenAIAgent
ITERATIONS=1000 dotnet run

# Python
cd python/azure_openai_agent  
ITERATIONS=1000 python main.py
```

#### Test Batch Processing
```bash
# .NET - Process in batches of 20
cd dotnet/OllamaAgent
TEST_MODE=batch BATCH_SIZE=20 ITERATIONS=100 dotnet run

# Python - Process in batches of 15
cd python/ollama_agent
TEST_MODE=batch BATCH_SIZE=15 ITERATIONS=100 python main.py
```

#### Test Concurrent Requests
```bash
# .NET - 10 concurrent requests
TEST_MODE=concurrent CONCURRENT_REQUESTS=10 ITERATIONS=50 dotnet run

# Python - 8 concurrent requests  
TEST_MODE=concurrent CONCURRENT_REQUESTS=8 ITERATIONS=50 python main.py
```

#### Test Streaming with TTFT
```bash
# Measure time-to-first-token in streaming responses
TEST_MODE=streaming ITERATIONS=100 dotnet run
TEST_MODE=streaming ITERATIONS=100 python main.py
```

#### Comprehensive Scenario Testing
```bash
# Test multiple prompt types (200 iterations each)
TEST_MODE=scenarios dotnet run
TEST_MODE=scenarios python main.py
```

## Completed Features

- ✅ **Basic Hello World Agents**: Simple implementations in C# and Python with 1000-iteration performance testing
- ✅ **Azure OpenAI Integration**: Cloud-based AI service integration
- ✅ **Ollama Integration**: Local model support for privacy and offline use
- ✅ **Comprehensive Performance Metrics**: Built-in time and memory tracking with statistical analysis
- ✅ **Model Warmup**: Automatic warmup calls to ensure consistent performance measurements
- ✅ **Metrics Export**: Automatic JSON export of performance data for easy comparison
- ✅ **Cross-platform**: Works on Windows, Linux, and macOS
- ✅ **Configuration via Environment Variables**: Easy setup with `.env` files
- ✅ **1000-Iteration Testing**: Statistically significant performance measurements
- ✅ **LLM-Ready Comparison**: AI-friendly format for automated performance analysis
- ✅ **Comprehensive Benchmarking Scenarios**: Multiple prompt types (simple, reasoning, conceptual, long output)
- ✅ **Batch Processing Tests**: Configurable batch processing with throughput metrics
- ✅ **CPU Utilization Monitoring**: Cross-platform CPU usage tracking
- ✅ **Automated Comparison Reports**: Enhanced JSON with summary generation
- ✅ **Streaming Response Tests**: Time-to-first-token measurement
- ✅ **Concurrent Request Handling**: Async concurrent execution using Task.WhenAll and asyncio.gather

## Contributing

Contributions are welcome! Areas of interest:

- New performance scenarios
- Additional AI service providers
- Improved metrics collection
- Optimization suggestions
- Cross-language performance analysis

## License

[License information to be added]

## Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Agent Framework Python GitHub](https://github.com/microsoft/agent-framework/tree/main/python)
- [Agent Framework .NET GitHub](https://github.com/microsoft/agent-framework/tree/main/dotnet)
- [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/ai-services/openai-service)
- [Ollama](https://ollama.ai/)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Python](https://www.python.org/downloads/)
