# Aspire-Based Web Application for Performance Comparison

## Overview

A new real-time web application has been added to the repository under the `aspire-web/` folder. This application uses **.NET Aspire** to orchestrate a full-stack solution for running and visualizing Microsoft Agent Framework performance comparisons between .NET and Python implementations.

## What's New

### 🎯 Key Features

1. **Real-time Performance Testing**
   - Run performance tests on both .NET and Python agents simultaneously
   - Configure test parameters through a web UI (iterations, model, test mode, etc.)
   - See live test results and comparisons

2. **Aspire Orchestration**
   - Unified service management with .NET Aspire
   - Integrated telemetry and monitoring
   - Service discovery and health checks
   - Aspire dashboard for observability

3. **Modern Web Stack**
   - **Frontend**: Blazor Server (interactive web UI)
   - **Backend (.NET)**: ASP.NET Core Web API
   - **Backend (Python)**: FastAPI
   - **Orchestrator**: .NET Aspire AppHost

4. **Comprehensive Metrics**
   - Execution time (total, average, min, max)
   - Memory usage
   - Machine information (CPU, RAM, OS)
   - Side-by-side comparison with performance winner

## Architecture

```
aspire-web/
├── AppHost/                    # Aspire Orchestrator
├── Web/                        # Blazor Frontend
├── DotNetBackend/              # .NET API Backend  
├── PythonBackend/              # Python FastAPI Backend
└── README.md                   # Detailed documentation
```

## Quick Start

```bash
# Navigate to aspire-web folder
cd aspire-web/AppHost/PerformanceComparison.AppHost

# Run with Aspire (starts all services)
dotnet run
```

This will:
1. Start the .NET backend API (port 5002)
2. Start the Blazor web frontend
3. Open the Aspire dashboard automatically
4. Provide unified monitoring and telemetry

**Note**: Python backend can be started separately:
```bash
cd aspire-web/PythonBackend
pip install -r requirements.txt
python main.py
```

## Prerequisites

- .NET 10.0 SDK or later
- Python 3.10+
- Ollama running locally (http://localhost:11434)
- A model pulled in Ollama (e.g., `ollama pull ministral-3`)

## How to Use

1. Run the AppHost (see Quick Start above)
2. Navigate to the web frontend URL shown in the terminal
3. Click on "Performance Comparison" in the navigation menu
4. Configure your test parameters:
   - Number of iterations (default: 10)
   - Model to use (default: ministral-3)
   - Endpoint (default: http://localhost:11434)
   - Test mode (standard, batch, concurrent, streaming, scenarios)
5. Click "Run Performance Tests"
6. View real-time results for both .NET and Python
7. See automatic comparison and performance winner

## What Makes This Different

### vs. Existing CLI Tests
- **Web UI** instead of command-line
- **Real-time visualization** vs. post-execution reports
- **Interactive configuration** vs. environment variables
- **Unified dashboard** with Aspire for monitoring

### Aspire Benefits
- **Service orchestration**: All services managed from one place
- **Built-in telemetry**: OpenTelemetry integration out of the box
- **Health monitoring**: Service status at a glance
- **Easy development**: Simplified local development experience
- **Production-ready**: Path to deployment with Aspire

## Configuration Options

The web UI provides configuration for:

| Parameter | Default | Description |
|-----------|---------|-------------|
| Iterations | 10 | Number of test iterations (1-1000) |
| Model | ministral-3 | Ollama model name |
| Endpoint | http://localhost:11434 | Ollama API endpoint |
| Test Mode | standard | Execution mode (standard, batch, concurrent, streaming, scenarios) |
| Batch Size | 10 | Items per batch (batch mode only) |
| Concurrent Requests | 5 | Parallel requests (concurrent mode only) |

## Documentation

For detailed documentation, see:
- **aspire-web/README.md** - Complete guide to the Aspire web application
- **docs/** - Existing performance comparison documentation

## Technology Stack

- **.NET 10.0** - Latest .NET framework
- **Aspire 13.0** - Service orchestration and observability
- **Blazor Server** - Interactive web UI
- **ASP.NET Core** - Web API backend
- **FastAPI** - Python backend
- **Microsoft.Agents.AI** - .NET Agent Framework
- **agent-framework-ollama** - Python Agent Framework
- **OpenTelemetry** - Distributed tracing and metrics
- **Bootstrap 5** - UI styling

## Future Enhancements

Potential areas for expansion:
- [ ] WebSocket support for real-time progress updates
- [ ] Historical test result storage and trending
- [ ] Chart visualizations (line charts, bar charts)
- [ ] Export results to CSV/JSON
- [ ] Comparison with historical baselines
- [ ] Multiple test configurations side-by-side
- [ ] Azure OpenAI backend support
- [ ] Docker containerization
- [ ] Kubernetes deployment manifests

## Integration with Existing Tests

The Aspire web application **complements** the existing CLI-based tests:

- **CLI tests** (`run_performance_tests.py`): Batch execution, automation, CI/CD
- **Web application**: Interactive testing, visualization, development

Both use the same underlying agent implementations, ensuring consistency.

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/)
- [Project README](aspire-web/README.md)

## Support

For issues or questions about the Aspire web application, please refer to:
1. The detailed README in `aspire-web/README.md`
2. Troubleshooting section in the documentation
3. Aspire dashboard logs when running locally

---

**Created**: January 2026  
**Status**: ✅ Ready to use  
**License**: MIT (same as main repository)
