# MAF-PerformanceComparison

Performance comparison of Microsoft Agent Framework implementations in Python vs .NET (C#).

## Overview

This project compares the performance and resource usage of Microsoft Agent Framework (based on Semantic Kernel) implementations across different programming languages and AI service providers.

## Project Structure

```
srcMAFPerformance/
├── dotnet/
│   └── HelloWorldAgent/          # C# Console Application
│       ├── Program.cs             # Main application code
│       └── HelloWorldAgent.csproj # Project file
└── python/
    └── hello_world_agent/         # Python Application
        ├── main.py                # Main application code
        └── requirements.txt       # Python dependencies
```

## Prerequisites

### For .NET (C#)
- .NET 8.0 SDK or later
- Microsoft.SemanticKernel NuGet package

### For Python
- Python 3.12 or later
- semantic-kernel package
- psutil package

## Getting Started

### Running the C# Hello World Agent

```bash
cd srcMAFPerformance/dotnet/HelloWorldAgent
dotnet build
dotnet run
```

### Running the Python Hello World Agent

```bash
cd srcMAFPerformance/python/hello_world_agent
pip install -r requirements.txt
python main.py
```

## Features

- **Basic Hello World Agents**: Simple implementations demonstrating the Microsoft Agent Framework in both languages
- **Performance Metrics**: Automatic measurement of:
  - Execution time (milliseconds)
  - Memory usage (MB)
- **Extensible Design**: Ready to integrate with:
  - Azure OpenAI Services
  - Local models via Ollama

## Performance Metrics

Each application measures and reports:
- **Execution Time**: How long the agent takes to initialize and respond
- **Memory Usage**: RAM consumed during execution

## Next Steps

- [ ] Implement Azure OpenAI integration
- [ ] Implement Ollama local model integration
- [ ] Add comprehensive benchmarking scenarios
- [ ] Create comparative analysis tools
- [ ] Document performance results

## Contributing

This is a performance comparison project. Contributions that add new scenarios, improve measurements, or optimize implementations are welcome.

## License

[License information to be added]