# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework (based on Semantic Kernel) implementations across different programming languages and AI service providers.

## Project Structure

```
srcMAFPerformance/
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
- .NET 8.0 SDK or later
- Microsoft.SemanticKernel NuGet package (automatically installed)

### For Python
- Python 3.12 or later
- semantic-kernel package
- psutil package (for performance measurement)
- python-dotenv package (for configuration)

### For Azure OpenAI Integration
- Azure OpenAI Service resource
- API endpoint and key
- Deployed model (e.g., gpt-4, gpt-3.5-turbo)

### For Ollama Integration
- Ollama installed locally ([Download here](https://ollama.ai/))
- At least one model pulled (e.g., `ollama pull llama2`)

## Getting Started

### 1. Basic Hello World Agents (Demo Mode)

These agents demonstrate the framework structure without requiring external services.

**C# Version:**
```bash
cd srcMAFPerformance/dotnet/HelloWorldAgent
dotnet build
dotnet run
```

**Python Version:**
```bash
cd srcMAFPerformance/python/hello_world_agent
pip install -r requirements.txt
python main.py
```

### 2. Azure OpenAI Agents

**Setup:**
1. Copy `.env.example` to `.env` in the respective agent directory
2. Fill in your Azure OpenAI credentials:
   ```
   AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
   AZURE_OPENAI_API_KEY=your-api-key
   AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4
   ```

**C# Version:**
```bash
cd srcMAFPerformance/dotnet/AzureOpenAIAgent
# Create .env file with your credentials
dotnet build
dotnet run
```

**Python Version:**
```bash
cd srcMAFPerformance/python/azure_openai_agent
# Create .env file with your credentials
pip install -r requirements.txt
python main.py
```

### 3. Ollama Agents (Local Models)

**Setup:**
1. Install Ollama from [https://ollama.ai/](https://ollama.ai/)
2. Start Ollama service
3. Pull a model: `ollama pull llama2`
4. (Optional) Copy `.env.example` to `.env` to customize endpoint or model

**C# Version:**
```bash
cd srcMAFPerformance/dotnet/OllamaAgent
dotnet build
dotnet run
```

**Python Version:**
```bash
cd srcMAFPerformance/python/ollama_agent
pip install -r requirements.txt
python main.py
```

## Performance Metrics

Each application automatically measures and reports:
- **Execution Time**: Time from initialization to completion (milliseconds)
- **Memory Usage**: RAM consumed during execution (MB)

Example output:
```
=== Performance Metrics ===
Execution Time: 1245 ms
Memory Used: 3.45 MB
========================
```

## Configuration

### Azure OpenAI Configuration

Set these environment variables or create a `.env` file:

| Variable | Description | Example |
|----------|-------------|---------|
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI service endpoint | `https://your-resource.openai.azure.com/` |
| `AZURE_OPENAI_API_KEY` | API key for authentication | `your-api-key-here` |
| `AZURE_OPENAI_DEPLOYMENT_NAME` | Deployed model name | `gpt-4` |

### Ollama Configuration

Set these environment variables or create a `.env` file (optional - defaults provided):

| Variable | Description | Default |
|----------|-------------|---------|
| `OLLAMA_ENDPOINT` | Ollama service endpoint | `http://localhost:11434` |
| `OLLAMA_MODEL_ID` | Model to use | `llama2` |

## Features

- ✅ **Basic Hello World Agents**: Simple implementations in C# and Python
- ✅ **Azure OpenAI Integration**: Cloud-based AI service integration
- ✅ **Ollama Integration**: Local model support for privacy and offline use
- ✅ **Automatic Performance Metrics**: Built-in time and memory tracking
- ✅ **Cross-platform**: Works on Windows, Linux, and macOS
- ✅ **Configuration via Environment Variables**: Easy setup with `.env` files

## Comparing Performance

To compare performance between implementations:

1. **Run the same scenario** in both C# and Python
2. **Compare the metrics** shown at the end of each run
3. **Consider factors** like:
   - Cold start vs. warm start
   - Model complexity
   - Network latency (for Azure OpenAI)
   - Hardware specifications

## Architecture

Both C# and Python implementations use:
- **Microsoft Semantic Kernel**: The foundation of the Microsoft Agent Framework
- **Modular Design**: Easy to add new scenarios or providers
- **Performance Monitoring**: Built-in metrics collection
- **Error Handling**: Graceful degradation when services are unavailable

## Troubleshooting

### Azure OpenAI Agents

**Problem**: "Configuration not found. Using demo mode."
- **Solution**: Create a `.env` file with your Azure OpenAI credentials

**Problem**: Connection errors or authentication failures
- **Solution**: Verify your endpoint URL and API key are correct

### Ollama Agents

**Problem**: "Could not connect to Ollama"
- **Solution**: Ensure Ollama is running (`ollama serve`) and the model is downloaded

**Problem**: "Model not found"
- **Solution**: Pull the model first: `ollama pull llama2`

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

- [Microsoft Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/ai-services/openai-service)
- [Ollama](https://ollama.ai/)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Python](https://www.python.org/downloads/)