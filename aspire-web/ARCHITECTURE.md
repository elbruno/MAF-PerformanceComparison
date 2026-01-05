# Aspire Performance Comparison - Architecture

## System Overview

```
┌────────────────────────────────────────────────────────────────┐
│                     User's Web Browser                          │
│                                                                  │
│  ┌──────────────────────────────────────────────────────┐     │
│  │  Blazor Server Web UI (Interactive)                  │     │
│  │  - Performance Dashboard                             │     │
│  │  - Configuration Form                                │     │
│  │  - Results Display                                   │     │
│  │  - Health Status                                     │     │
│  └──────────────────────────────────────────────────────┘     │
└──────────────────────┬─────────────────────────────────────────┘
                       │ HTTPS
                       ▼
        ┌──────────────────────────────────┐
        │   Aspire AppHost Orchestrator     │
        │   - Service Discovery             │
        │   - Health Monitoring             │
        │   - Telemetry Collection          │
        │   - Unified Dashboard             │
        └──────────┬───────────────┬────────┘
                   │               │
        ┌──────────▼────┐   ┌──────▼──────────┐
        │ .NET Backend  │   │ Python Backend  │
        │ (Port 5002)   │   │ (Port 5001)     │
        │               │   │                 │
        │ ASP.NET Core  │   │ FastAPI         │
        │ Web API       │   │ Application     │
        └──────┬────────┘   └────────┬────────┘
               │                     │
               │  Both communicate   │
               │  with Ollama        │
               │                     │
               └──────────┬──────────┘
                          ▼
                ┌─────────────────────┐
                │  Ollama Service     │
                │  (localhost:11434)  │
                │                     │
                │  - ministral-3      │
                │  - Other models     │
                └─────────────────────┘
```

## Component Details

### 1. User Interface Layer

**Blazor Web Application**
- **Technology**: Blazor Server with .NET 10.0
- **Features**: 
  - Interactive forms
  - Real-time updates
  - Bootstrap 5 styling
  - Responsive design

**Key Pages**:
- `/` - Home
- `/dashboard` - Performance Comparison Dashboard
- `/counter` - Sample counter
- `/weather` - Sample weather

### 2. Orchestration Layer

**Aspire AppHost**
- **Technology**: .NET Aspire 13.0
- **Purpose**: Service management and coordination
- **Features**:
  - Launches all services
  - Monitors service health
  - Provides unified dashboard
  - Collects telemetry

**Configuration**:
```csharp
var dotnetBackend = builder.AddProject("dotnet-backend", ...)
var web = builder.AddProject("web", ...)
    .WithEnvironment("DotNetBackend", "http://localhost:5002")
    .WithEnvironment("PythonBackend", "http://localhost:5001");
```

### 3. Backend API Layer

#### .NET Backend (ASP.NET Core)

**Responsibilities**:
- Execute performance tests using Microsoft.Agents.AI
- Collect metrics (time, memory, machine info)
- Provide REST API endpoints

**API Endpoints**:
```
POST /api/performance/run
  Body: TestConfiguration
  Response: TestResult

GET /api/performance/health
  Response: { status: "healthy", service: "dotnet-backend" }
```

**Test Flow**:
1. Receive test configuration
2. Initialize Ollama agent
3. Perform warmup call
4. Run N iterations
5. Collect metrics
6. Return results

#### Python Backend (FastAPI)

**Responsibilities**:
- Execute performance tests using agent-framework-ollama
- Collect metrics (time, memory, machine info)
- Provide REST API endpoints

**API Endpoints**:
```
POST /api/performance/run
  Body: TestConfiguration
  Response: TestResult

GET /api/performance/health
  Response: { status: "healthy", service: "python-backend" }
```

**Test Flow**:
1. Receive test configuration
2. Initialize Ollama agent
3. Perform warmup call
4. Run N iterations
5. Collect metrics
6. Return results

### 4. AI Service Layer

**Ollama**
- **Purpose**: Local LLM inference
- **Default Port**: 11434
- **Models**: ministral-3, llama2, mistral, etc.
- **Protocol**: HTTP REST API

## Data Flow

### Performance Test Execution

```
User Browser
    │
    │ 1. Configure test (iterations, model, etc.)
    │ 2. Click "Run Performance Tests"
    ▼
Blazor Frontend
    │
    │ 3. POST request to both backends (parallel)
    ├──────────────┬──────────────┐
    ▼              ▼              ▼
.NET Backend  Python Backend  (Concurrent)
    │              │
    │ 4. Initialize agents with Ollama
    ▼              ▼
  Ollama ◄────── Ollama
    │              │
    │ 5. Run iterations
    │              │
    ▼              ▼
.NET Backend  Python Backend
    │              │
    │ 6. Collect metrics
    │              │
    └──────┬───────┘
           ▼
    Blazor Frontend
           │
           │ 7. Display results
           │ 8. Show comparison
           ▼
    User Browser
```

### Health Check Flow

```
User Browser
    │
    │ Click "Refresh Status"
    ▼
Blazor Frontend
    │
    │ GET /api/performance/health (parallel)
    ├──────────────┬──────────────┐
    ▼              ▼              ▼
.NET Backend  Python Backend
    │              │
    │ Return health status
    └──────┬───────┘
           ▼
    Blazor Frontend
           │
           │ Update status badges
           ▼
    User Browser
```

## Configuration Flow

### User Configuration

```
User Input (Web Form)
    ├─ Iterations: 10
    ├─ Model: ministral-3
    ├─ Endpoint: http://localhost:11434
    ├─ Test Mode: standard
    ├─ Batch Size: 10 (if batch mode)
    └─ Concurrent Requests: 5 (if concurrent mode)
    
    ↓ Converted to
    
TestConfiguration Object
    {
      "iterations": 10,
      "model": "ministral-3",
      "endpoint": "http://localhost:11434",
      "testMode": "standard",
      "batchSize": 10,
      "concurrentRequests": 5
    }
    
    ↓ Sent to
    
Backend APIs (both receive same config)
    
    ↓ Used to
    
Initialize Agent & Run Test
```

## Deployment Architecture

### Local Development (Current)

```
Developer Machine
├── Aspire AppHost (manages all)
│   ├── Blazor Web (auto-started)
│   ├── .NET Backend (auto-started)
│   └── Python Backend (manual start)
└── Ollama Service (runs independently)
```

### Production (Future)

```
Cloud Infrastructure
├── Container Orchestrator (Kubernetes/Azure Container Apps)
│   ├── Aspire AppHost Container
│   ├── Blazor Web Container
│   ├── .NET Backend Container
│   └── Python Backend Container
├── Load Balancer
└── Ollama Service (dedicated instance or cloud API)
```

## Security Architecture

```
User ──HTTPS──> Web Frontend
                    │
                    │ Internal Network
                    ├──HTTP──> .NET Backend
                    └──HTTP──> Python Backend
                                    │
                                    │ Internal Network
                                    └──HTTP──> Ollama
```

**Security Layers**:
1. **Frontend**: HTTPS, Anti-forgery tokens
2. **Backend APIs**: CORS configured, Input validation
3. **Internal Network**: Service-to-service communication
4. **No Secrets**: No API keys or passwords in code

## Observability Architecture

```
All Services
    │
    │ Emit Telemetry
    ├─── Logs
    ├─── Metrics
    └─── Traces
    
    ↓ Collected by
    
Aspire Dashboard
    ├─── Resources View (service list)
    ├─── Console Logs (per service)
    ├─── Metrics (performance data)
    └─── Traces (distributed tracing)
    
    ↓ Viewed by
    
Developer/Operator
```

## Technology Stack Summary

| Layer | Technology | Version |
|-------|-----------|---------|
| **Orchestration** | .NET Aspire | 13.0 |
| **Frontend** | Blazor Server | .NET 10.0 |
| **Backend (.NET)** | ASP.NET Core Web API | .NET 10.0 |
| **Backend (Python)** | FastAPI | Latest |
| **Agent Framework (.NET)** | Microsoft.Agents.AI | 1.0.0-preview |
| **Agent Framework (Python)** | agent-framework-ollama | Latest |
| **AI Service** | Ollama | Latest |
| **UI Framework** | Bootstrap | 5.x |

## Extensibility Points

The architecture is designed to be extensible:

1. **Add New Backends**: Easy to add more language implementations
2. **Add New Test Modes**: Extend TestConfiguration and backend logic
3. **Add New Metrics**: Extend TestMetrics model
4. **Add New Visualizations**: Add new Blazor components
5. **Add Authentication**: Use Aspire's auth features
6. **Add Persistence**: Connect database for history
7. **Add Notifications**: Integrate SignalR for real-time updates

## Performance Considerations

**Scalability**:
- Blazor Server: Maintains WebSocket per user
- Backend APIs: Stateless, can scale horizontally
- Ollama: Resource-intensive, may need GPU

**Optimization**:
- Async/await throughout for non-blocking I/O
- Minimal state management
- Efficient serialization
- Connection pooling (HTTP clients)

**Resource Usage**:
- Memory: ~100-200 MB (all services)
- CPU: Varies with test load
- Network: Minimal between services

---

This architecture provides a solid foundation for real-time performance comparison while remaining flexible for future enhancements.
