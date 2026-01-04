# Performance Comparison Web Application (.NET Aspire)

This is a real-time performance comparison web application built with .NET Aspire that orchestrates multiple services to run and visualize Microsoft Agent Framework performance tests.

## ✨ Latest Updates (v2.0)

**Background Test Execution with Real-time Polling:**
- Tests now run in background processes for non-blocking operation
- Auto-polling every 2 seconds for real-time status updates
- Collapsible UI sections for cleaner interface
- Export results as JSON files
- Stop running tests at any time
- See [CHANGELOG.md](CHANGELOG.md) for full details

## Architecture

```
┌──────────────────────────────────────────────────────┐
│  User Browser (Blazor Frontend)                      │
│  - Polls status every 2 seconds                      │
│  - Displays real-time progress                       │
└────────┬─────────────────────────────────────────────┘
         │ HTTP Polling
         ▼
┌──────────────────────────────────────────────────────┐
│  Aspire AppHost (Orchestrator)                       │
│  - Service discovery & health monitoring             │
└────────┬─────────────────────────────────────────────┘
         │
    ┌────┴──────────────────────┐
    │                           │
    ▼                           ▼
┌─────────────┐         ┌───────────────────┐
│ .NET Backend│         │ Python Backend    │
│ Background  │         │ Background        │
│ Test Process│         │ Test Process      │
└─────────────┘         └───────────────────┘
```

## Key Features

- **Real-time Updates**: Status polls every 2 seconds during test execution
- **Background Execution**: Tests run in separate processes, non-blocking
- **Start/Stop Control**: Start tests and stop them at any time
- **Collapsible UI**: Click any card header to collapse/expand
- **Export Results**: Download complete test results as JSON
- **Progress Tracking**: Visual progress bars and percentage indicators
- **Live Metrics**: See current iteration, elapsed time, and performance metrics
- **Clean Interface**: Focused dashboard with no unnecessary pages

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
- **Route**: `/dashboard` (auto-redirects from `/`)
- **Purpose**: Interactive web UI for running tests and viewing real-time results
- **Features**:
  - Configure test parameters (iterations, model, endpoint)
  - Start/stop tests on both .NET and Python backends simultaneously
  - **Real-time status polling** (every 2 seconds)
  - Progress bars showing test completion
  - **Collapsible sections** for cleaner interface
  - **Export results** to JSON file
  - Side-by-side comparison of results
  - Service health monitoring

### 3. .NET Backend API
- **Project**: `PerformanceComparison.DotNetBackend`
- **Port**: 5002
- **Purpose**: Execute performance tests using .NET Agent Framework
- **Endpoints**:
  - `POST /api/performance/start` - Start test in background
  - `POST /api/performance/stop` - Stop running test
  - `GET /api/performance/status` - Get real-time test status
  - `GET /api/performance/health` - Health check
- **Features**:
  - **Background test execution** with cancellation support
  - Runs agent tests with Microsoft.Agents.AI
  - Collects detailed performance metrics
  - Machine information gathering
  - Session-based tracking

### 4. Python Backend API
- **Project**: `PythonBackend` (FastAPI)
- **Port**: 5001
- **Purpose**: Execute performance tests using Python Agent Framework
- **Endpoints**:
  - `POST /api/performance/start` - Start test in background
  - `POST /api/performance/stop` - Stop running test
  - `GET /api/performance/status` - Get real-time test status
  - `GET /api/performance/health` - Health check
- **Features**:
  - **Background test execution** with async tasks
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
- **Performance Dashboard**: Automatically loads at `/dashboard` (or click "Performance Dashboard" in menu)

## How to Use

### Running a Performance Test

1. **Configure Parameters**:
   - Set number of iterations (1-10,000)
   - Choose model name (e.g., `ministral-3`)
   - Set Ollama endpoint URL
   
2. **Start Tests**:
   - Click the green "Start Tests" button
   - Both .NET and Python tests start simultaneously in background
   - Status automatically updates every 2 seconds

3. **Monitor Progress**:
   - Watch real-time progress bars
   - See current iteration count
   - View elapsed time
   - Monitor live metrics (avg/min/max times)

4. **Stop Tests** (Optional):
   - Click red "Stop Tests" button anytime to cancel
   - Tests gracefully stop and show partial results

5. **View Results**:
   - When completed, see metrics for both implementations
   - Comparison summary shows which is faster
   - All cards are collapsible (click header to toggle)

6. **Export Results**:
   - Click "Export Results" button
   - Downloads JSON file with complete test data
   - Includes configuration, metrics, and machine info

### UI Features

**Collapsible Cards**: Click any card header to collapse/expand:
- Test Configuration
- Service Status
- .NET Results
- Python Results
- Comparison Summary

**Real-time Updates**: During test execution you see:
- Progress percentage and visual progress bar
- Current iteration / Total iterations
- Elapsed time in seconds
- Running average time per iteration
- Min/max iteration times
- Memory usage

**Action Buttons**:
- 🟢 **Start Tests**: Begin performance tests
- 🔴 **Stop Tests**: Cancel running tests
- 🔵 **Export Results**: Download JSON results
- 🔄 **Refresh Status**: Update service health

## Configuration

### Test Parameters

The web interface allows you to configure:

- **Iterations**: Number of test iterations (default: 10, max: 10000)
- **Model**: Ollama model name (default: `ministral-3`)
- **Endpoint**: Ollama API endpoint (default: `http://localhost:11434`)

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
