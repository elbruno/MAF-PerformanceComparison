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

Each application automatically runs **1000 iterations** of agent operations to measure performance accurately. The following metrics are collected and reported:

- **Total Iterations**: Number of agent operations performed (1000)
- **Total Execution Time**: Time from initialization to completion (milliseconds)
- **Average Time per Iteration**: Mean execution time per agent operation (milliseconds)
- **Min Iteration Time**: Fastest iteration time (milliseconds)
- **Max Iteration Time**: Slowest iteration time (milliseconds)
- **Memory Usage**: RAM consumed during execution (MB)

Example output:

```
=== Performance Metrics ===
Total Iterations: 1000
Total Execution Time: 1245 ms
Average Time per Iteration: 1.234 ms
Min Iteration Time: 0.985 ms
Max Iteration Time: 2.156 ms
Memory Used: 3.45 MB
========================
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
- ✅ **Cross-platform**: Works on Windows, Linux, and macOS
- ✅ **Configuration via Environment Variables**: Easy setup with `.env` files
- ✅ **1000-Iteration Testing**: Statistically significant performance measurements

## Comparing Performance

To compare performance between implementations:

1. **Run the same scenario** in both C# and Python
2. **Compare the metrics** shown at the end of each run, focusing on:
   - Total execution time for 1000 iterations
   - Average time per iteration
   - Memory usage patterns
3. **Consider factors** like:
   - Cold start vs. warm start
   - Model complexity
   - Network latency (for Azure OpenAI)
   - Hardware specifications
   - Language runtime differences (JIT compilation, GC behavior, etc.)

The 1000-iteration approach provides statistically significant results by averaging out transient performance fluctuations.

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

## Next Steps

- [ ] Add comprehensive benchmarking scenarios
- [ ] Implement batch processing tests
- [ ] Add CPU utilization monitoring
- [ ] Create automated comparison reports
- [ ] Add more AI service providers (OpenAI, Anthropic, etc.)
- [ ] Implement streaming response tests
- [ ] Add concurrent request handling tests

## Contributing

Contributions are welcome! Areas of interest:

- New performance scenarios
- Additional AI service providers
- Improved metrics collection
- Optimization suggestions

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
