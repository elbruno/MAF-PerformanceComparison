# Performance Comparison Web Application (.NET Aspire)

This is a real-time performance comparison web application built with .NET Aspire that orchestrates multiple services to run and visualize Microsoft Agent Framework performance tests.

## Architecture

```
┌──────────────────────────────────────────────────────┐
│  Aspire AppHost (Orchestrator)                       │
│  - Service discovery                                 │
│  - Health monitoring                                 │
│  - Telemetry collection                              │
└────────┬─────────────────────────────────────────────┘
         │
    ┌────┴──────────────────────┐
    │                           │
    ▼                           ▼
┌─────────────┐         ┌───────────────────┐
│ Blazor Web  │         │ API Backends      │
│ Frontend    │◄────────┤ - .NET Backend    │
│             │  HTTP   │ - Python Backend  │
└─────────────┘         └───────────────────┘
```

## Components

### 1. AppHost (Orchestrator)
- **Project**: `PerformanceComparison.AppHost`
- **Purpose**: Aspire orchestration and service discovery
- **Port**: Default Aspire dashboard port
- **Features**:
  - Launches and manages all services
  - Provides unified dashboard for monitoring
  - Collects distributed telemetry

### 2. Blazor Web Frontend
- **Project**: `PerformanceComparison.Web`
- **Purpose**: Interactive web UI for running tests and viewing results
- **Features**:
  - Configure test parameters (iterations, model, endpoint, etc.)
  - Run tests on both .NET and Python backends simultaneously
  - Real-time progress updates
  - Side-by-side comparison of results
  - Service health monitoring

### 3. .NET Backend API
- **Project**: `PerformanceComparison.DotNetBackend`
- **Port**: 5002
- **Purpose**: Execute performance tests using .NET Agent Framework
- **Endpoints**:
  - `POST /api/performance/run` - Run performance test
  - `GET /api/performance/health` - Health check
- **Features**:
  - Runs agent tests with Microsoft.Agents.AI
  - Collects detailed performance metrics
  - Machine information gathering
  - Progress reporting

### 4. Python Backend API
- **Project**: `PythonBackend` (FastAPI)
- **Port**: 5001
- **Purpose**: Execute performance tests using Python Agent Framework
- **Endpoints**:
  - `POST /api/performance/run` - Run performance test
  - `GET /api/performance/health` - Health check
- **Features**:
  - Runs agent tests with agent-framework-ollama
  - Collects detailed performance metrics
  - Machine information gathering

## Prerequisites

1. **.NET 10.0 SDK** or later
2. **Python 3.10+**
3. **Ollama** running locally at `http://localhost:11434`
4. A model pulled in Ollama (default: `ministral-3`)

```bash
# Install Ollama from https://ollama.com/
# Pull a model
ollama pull ministral-3
```

## Quick Start

### Option 1: Run with Aspire AppHost (Recommended)

This will start all services with the Aspire dashboard:

```bash
cd aspire-web/AppHost/PerformanceComparison.AppHost
dotnet run
```

The Aspire dashboard will open automatically, showing all services and their status.

### Option 2: Run Services Individually

#### Terminal 1: .NET Backend
```bash
cd aspire-web/DotNetBackend/PerformanceComparison.DotNetBackend
dotnet run
```

#### Terminal 2: Python Backend
```bash
cd aspire-web/PythonBackend
pip install -r requirements.txt
python main.py
```

#### Terminal 3: Web Frontend
```bash
cd aspire-web/Web/PerformanceComparison.Web
dotnet run
```

### Accessing the Application

- **Web Frontend**: Navigate to the URL shown in the terminal (usually `https://localhost:5xxx`)
- **Aspire Dashboard**: Opens automatically when running AppHost
- **Performance Dashboard**: Click "Performance Comparison" in the navigation menu

## Configuration

### Test Parameters

The web interface allows you to configure:

- **Iterations**: Number of test iterations (default: 10, max: 1000)
- **Model**: Ollama model name (default: `ministral-3`)
- **Endpoint**: Ollama API endpoint (default: `http://localhost:11434`)
- **Test Mode**: 
  - `standard` - Sequential execution
  - `batch` - Batch processing
  - `concurrent` - Parallel requests
  - `streaming` - Streaming responses
  - `scenarios` - Multiple prompt types
- **Batch Size**: For batch mode (default: 10)
- **Concurrent Requests**: For concurrent mode (default: 5)

### Environment Variables

You can also configure via environment variables:

```bash
# .NET Backend
export OLLAMA_ENDPOINT="http://localhost:11434"
export OLLAMA_MODEL_NAME="ministral-3"

# Python Backend
export OLLAMA_HOST="http://localhost:11434"
export OLLAMA_CHAT_MODEL_ID="ministral-3"
```

## Features

### Real-time Performance Testing
- Run tests on both .NET and Python implementations simultaneously
- See live progress and status updates
- Instant results comparison

### Comprehensive Metrics
- Total execution time
- Average time per iteration
- Min/Max iteration times
- Memory usage
- Machine information (CPU, RAM, OS)
- Warmup success status

### Side-by-Side Comparison
- Automatic comparison of .NET vs Python performance
- Performance winner calculation
- Percentage difference analysis

### Service Health Monitoring
- Real-time health status for all backends
- Refresh health status on demand
- Visual indicators (green/red badges)

## Building the Solution

```bash
cd aspire-web
dotnet restore
dotnet build
```

## Project Structure

```
aspire-web/
├── AppHost/                              # Aspire orchestrator
│   └── PerformanceComparison.AppHost/
│       ├── Program.cs                    # Aspire configuration
│       └── PerformanceComparison.AppHost.csproj
├── Web/                                  # Blazor frontend
│   └── PerformanceComparison.Web/
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── PerformanceDashboard.razor  # Main dashboard
│       │   └── Layout/
│       ├── Models/                       # Data models
│       ├── Services/                     # API clients
│       └── PerformanceComparison.Web.csproj
├── DotNetBackend/                        # .NET API backend
│   └── PerformanceComparison.DotNetBackend/
│       ├── Controllers/                  # API controllers
│       ├── Models/                       # Data models
│       ├── Services/                     # Performance test service
│       └── PerformanceComparison.DotNetBackend.csproj
├── PythonBackend/                        # Python FastAPI backend
│   ├── main.py                           # FastAPI application
│   └── requirements.txt                  # Python dependencies
├── PerformanceComparison.sln             # Solution file
└── README.md                             # This file
```

## Development

### Adding New Test Modes

1. Update `TestConfiguration` model in both backends
2. Implement test mode logic in `PerformanceTestService.cs` (.NET) and `main.py` (Python)
3. Update UI in `PerformanceDashboard.razor` to include new mode option

### Adding New Metrics

1. Add fields to `TestMetrics` model
2. Collect metrics in test services
3. Display in dashboard UI

## Troubleshooting

### Ollama Not Running
**Error**: "Could not connect to Ollama"
**Solution**: Start Ollama and ensure it's running on `http://localhost:11434`

### Model Not Found
**Error**: "Model not found"
**Solution**: Pull the model with `ollama pull ministral-3`

### Port Already in Use
**Error**: "Address already in use"
**Solution**: Change ports in `launchSettings.json` or stop conflicting services

### Backend Not Responding
**Solution**: Check the Aspire dashboard for service status and logs

## Performance Tips

1. **Start with low iterations** (10-50) to test the setup
2. **Increase gradually** to desired test size
3. **Monitor system resources** during tests
4. **Run tests when system is idle** for more accurate results

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/)
- [Ollama Documentation](https://ollama.com/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [FastAPI Documentation](https://fastapi.tiangolo.com/)

## License

MIT License - See LICENSE file in repository root
