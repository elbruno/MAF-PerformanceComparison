# GitHub Copilot Instructions for MAF-PerformanceComparison

## Project Overview

**MAF-PerformanceComparison** is a benchmarking suite that compares Microsoft Agent Framework (MAF) implementations across Python and .NET. It measures performance, resource usage, and behavior patterns across multiple test modes and AI providers (Ollama, Azure OpenAI).

### Big Picture Architecture

```
┌─────────────────────────────────────────────────────┐
│  Test Execution Layer (run_tests.ps1/sh/bat)       │
│  - Orchestrates .NET and Python tests               │
│  - Cleans old metrics before each run               │
│  - Sets environment variables (ITERATIONS, etc.)   │
└──────────────┬──────────────────────────────────────┘
               │
    ┌──────────┴──────────┐
    ▼                     ▼
┌──────────────┐   ┌──────────────────┐
│ .NET Tests   │   │ Python Tests     │
│ (Program.cs) │   │ (main.py)        │
│ - Agent init │   │ - Agent init     │
│ - Warmup     │   │ - Warmup         │
│ - Iterations │   │ - Iterations     │
│ - Machine    │   │ - Machine        │
│   info       │   │   info           │
└──────┬───────┘   └──────┬───────────┘
       │                  │
       └──────────────┬───┘
                      ▼
          ┌───────────────────────┐
          │ Metrics JSON Files    │
          │ (metrics_*.json)      │
          │ - TestInfo            │
          │ - MachineInfo         │
          │ - Metrics             │
          └───────┬───────────────┘
                  │
                  ▼
        ┌──────────────────────────┐
        │ process_results_ollama.py│
        │ - Find metrics           │
        │ - Move to tests_results/ │
        │ - Generate reports      │
        │ - AI analysis           │
        └──────┬───────────────────┘
               │
       ┌───────┴────────┐
       ▼                ▼
   ┌────────┐    ┌─────────────────┐
   │ Reports│    │ Analysis        │
   │comparison  │ (AI-generated)  │
   │_report.md │ analysis_report  │
   └────────┘    │ .md             │
                 └─────────────────┘
```

## Critical Workflows

### 1. Running Tests (No Git Needed)

**Standard workflow:**

```powershell
# Windows PowerShell
.\run_tests.ps1 -Iterations 1000 -TestMode standard -AgentType Ollama

# Linux/macOS
./run_tests.sh

# Windows Command Prompt
run_tests.bat
```

**Environment Variables** (set before running agents directly):

- `ITERATIONS` - Number of test repetitions (default: 1000)
- `TEST_MODE` - `standard`, `batch`, `concurrent`, `streaming`, `scenarios`
- `BATCH_SIZE` - Items per batch when TEST_MODE=batch (default: 10)
- `CONCURRENT_REQUESTS` - Parallel requests when TEST_MODE=concurrent (default: 5)
- `OLLAMA_HOST` - Ollama endpoint (default: http://localhost:11434)
- `OLLAMA_MODEL_NAME` / `OLLAMA_CHAT_MODEL_ID` - Model name (default: ministral-3)

**Key Behavior:**

- Scripts auto-clean old metrics before each run
- Tests generate `metrics_*.json` in agent directories
- `process_results_ollama.py` auto-runs if tests pass
- Results organized in `tests_results/<timestamp>_<mode>_<iterations>iter/`

### 2. Building .NET Agents

**Requirements:**

- .NET 10.0 SDK or later
- NuGet packages: Microsoft.Agents.AI, OllamaSharp, System.Management

**Build & Run:**

```powershell
cd dotnet/OllamaAgent
dotnet build
$env:ITERATIONS=100; dotnet run
```

**Key Dependencies in `.csproj`:**

- `Microsoft.Agents.AI` - Core framework
- `OllamaSharp` - Ollama client
- `System.Management` - WMI queries for machine info (Windows)

### 3. Running Python Agents

**Requirements:**

- Python 3.10+
- Packages: `agent-framework-ollama`, `psutil`, `python-dotenv`

**Run:**

```bash
cd python/ollama_agent
pip install -r requirements.txt
export ITERATIONS=100
python main.py
```

**Environment file:** Create `.env` with:

```
OLLAMA_HOST=http://localhost:11434
OLLAMA_CHAT_MODEL_ID=ministral-3
```

### 4. Processing & Analyzing Results

**Automatic:** `run_tests.ps1` auto-invokes this.

**Manual:**

```bash
python process_results_ollama.py
```

**Output:**

- Moves metrics to `tests_results/` timestamped folder
- Creates `comparison_report.md` (formatted metrics + LLM prompt)
- Creates `analysis_report.md` (AI-generated analysis using test model)

## Project-Specific Conventions

### Metrics JSON Structure

Every test exports metrics in this format:

```json
{
  "TestInfo": {
    "Language": "CSharp|Python",
    "Framework": "DotNet|Python",
    "Provider": "Ollama|AzureOpenAI",
    "Model": "ministral-3",
    "Endpoint": "http://localhost:11434",
    "Timestamp": "ISO-8601",
    "WarmupSuccessful": true
  },
  "MachineInfo": {
    "OSSystem": "Windows",
    "ProcessorCount": 32,
    "TotalMemoryGB": 31.71,
    "CPUModel": "Intel Core i9-14900KF",
    "GPUModel": "NVIDIA RTX 3080"
  },
  "Metrics": {
    "TotalIterations": 1000,
    "TotalExecutionTimeMs": 193340,
    "AverageTimePerIterationMs": 193.34,
    "MinIterationTimeMs": 122.21,
    "MaxIterationTimeMs": 957.14,
    "MemoryUsedMB": 16.23
  }
}
```

### Agent Initialization Pattern

**Both .NET and Python follow this pattern:**

1. **Load config from env** → Default fallback
2. **Warmup call** → Prepare model/framework
3. **Iteration loop** → Timed requests
4. **Collect machine info** → Before export
5. **Export metrics** → JSON with timestamp

**Example (.NET):**

```csharp
var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL_NAME") ?? "ministral-3";
var agent = new OllamaApiClient(endpoint, model).CreateAIAgent(...);
// Warmup, then iterations, then export with MachineInfo
```

**Example (Python):**

```python
model = os.getenv("OLLAMA_CHAT_MODEL_ID", "ministral-3")
agent = OllamaChatClient(model_id=model).create_agent(...)
# Warmup, then iterations, then export with MachineInfo
```

### File Organization

- `dotnet/[Agent]` - Agent implementations (.NET)
- `python/[agent]` - Agent implementations (Python)
- `docs/` - **All guides, templates, and documentation** (centralized location)
  - `docs/MACHINE_INFO_GUIDE.md` - Machine information interpretation
  - `docs/comparison_prompt_template.md` - LLM analysis structure
  - `docs/IMPLEMENTATION_SUMMARY.md` - Architecture deep-dive
  - `docs/VERIFICATION_CHECKLIST.md` - Implementation status
  - `docs/DETAILED_GUIDE.md` - Comprehensive documentation
  - `docs/SCRIPTS_README.md` - Test automation details
  - `docs/MACHINE_INFO_CHANGELOG.md` - Machine info implementation details
- `tests_results/` - Organized test output (created by processor, auto-managed)
- `run_tests.*` - Multi-platform test orchestration (root level only)

### Naming Conventions

- **Metrics files:** `metrics_<language>_<provider>_<timestamp>.json`
  - Example: `metrics_dotnet_ollama_20260104_003410.json`
- **Reports:** `comparison_report_<mode>_<iter>iter.md`, `analysis_report_<mode>_<iter>iter.md`
- **Environments:** `<agent_type>_agent` (lowercase, no dots)

## Integration Points & Dependencies

### External Services

- **Ollama API** - Default AI provider

  - Endpoint: `http://localhost:11434` (configurable)
  - Requires local Ollama installation
  - Models via `ollama pull <model>`

- **Azure OpenAI** (optional)
  - Set up via `.env` with Azure credentials
  - Used in `AzureOpenAIAgent` implementations

### Key Libraries

**Python:**

- `agent-framework-ollama` - Microsoft Agent Framework (Ollama backend)
- `psutil` - System metrics (CPU, memory, GPU detection)
- `python-dotenv` - Environment variable loading

**.NET:**

- `Microsoft.Agents.AI` - Core framework
- `OllamaSharp` - Ollama HTTP client
- `System.Management` - WMI for Windows system info (optional, graceful fallback)

### Data Flow

1. **Test Run** → Generates `metrics_*.json` (in agent dir)
2. **process_results_ollama.py** → Discovers metrics, moves to `tests_results/`
3. **Report Generation** → Creates markdown comparison + AI prompts
4. **Ollama Agent** → Analyzes metrics, generates insights
5. **Output** → `comparison_report.md` + `analysis_report.md` in timestamped folder

## When Modifying This Codebase

### Adding New Test Mode

1. Update `run_tests.ps1|sh|bat` with new `TestMode` case
2. Implement in both `dotnet/*/Program.cs` and `python/*/main.py` with timing logic
3. Export metrics with new fields in same JSON structure
4. Update `process_results_ollama.py` if new report formatting needed
5. Document in `docs/comparison_prompt_template.md`

### Adding New Metrics

1. Collect in agent code (before JSON export)
2. Add to `Metrics` section of JSON (or create new top-level section)
3. Update `process_results_ollama.py` markdown display
4. Add interpretation guidance in `docs/MACHINE_INFO_GUIDE.md`

### Cross-Platform Issues

- **Machine Info:** Use platform detection (avoid WMI on Linux)
  - `.NET` Example: `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`
  - **Python** Example: `platform.system() == "Windows"`
- **Paths:** Use `Path.Combine` (.NET) or `os.path.join` (Python)
- **Shell Scripts:** Keep both PowerShell (ps1) and Bash (sh) versions in sync

## Documentation References

**All documentation is located in the `docs/` folder:**

- [docs/MACHINE_INFO_GUIDE.md](docs/MACHINE_INFO_GUIDE.md) - Metrics interpretation and machine information
- [docs/comparison_prompt_template.md](docs/comparison_prompt_template.md) - LLM analysis structure and prompts
- [docs/IMPLEMENTATION_SUMMARY.md](docs/IMPLEMENTATION_SUMMARY.md) - Architecture deep-dive and feature details
- [docs/VERIFICATION_CHECKLIST.md](docs/VERIFICATION_CHECKLIST.md) - Implementation verification status
- [docs/DETAILED_GUIDE.md](docs/DETAILED_GUIDE.md) - Comprehensive configuration and advanced usage
- [docs/SCRIPTS_README.md](docs/SCRIPTS_README.md) - Test automation and script details
- [docs/MACHINE_INFO_CHANGELOG.md](docs/MACHINE_INFO_CHANGELOG.md) - Machine info collection implementation

**Root-level reference files:**

- [README.md](README.md) - Quick start guide
- [.github/copilot-instructions.md](.github/copilot-instructions.md) - This file (AI agent guidance)

## Quick Diagnostic Commands

```powershell
# Check if Ollama is running
curl http://localhost:11434/api/tags

# List available models
ollama list

# Run a specific test
cd dotnet/OllamaAgent; $env:ITERATIONS=3; dotnet run

# Process results manually
python process_results_ollama.py

# View latest metrics
Get-Content tests_results/*/metrics_*.json | ConvertFrom-Json | ConvertTo-Json -Depth 10
```
