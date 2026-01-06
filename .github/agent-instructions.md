# AI Agent Instructions for MAF-PerformanceComparison

## Critical Documentation Rule

**⚠️ MANDATORY: All documentation files MUST reside in the `docs/` folder.**

- **Never** create markdown files in the root directory (except README.md)
- **Never** create documentation in agent folders (dotnet/*, python/*)
- **Never** create documentation in test results folders
- **Always** place guides, templates, summaries, and reports in `docs/`
- **Always** update existing docs in `docs/` rather than creating duplicates elsewhere

### Approved Documentation Locations

✅ **Allowed:**

- `docs/` - All documentation, guides, templates, and reference materials
- `README.md` - Project quick start guide (root level only)
- `.github/copilot-instructions.md` - GitHub Copilot guidance
- `.github/agent-instructions.md` - This file (AI agent guidance)

❌ **Not Allowed:**

- `IMPLEMENTATION_*.md`, `SUMMARY_*.md`, `GUIDE_*.md` in root
- Documentation files in `scenario-*/` directories
- Documentation files in `dotnet/` or `python/` agent folders
- Any `.md` files outside `docs/` (except the approved locations above)

## Project Overview

**MAF-PerformanceComparison** is a benchmarking suite comparing Microsoft Agent Framework (MAF) implementations across Python and .NET. It measures performance, resource usage, and behavior patterns using multiple test modes and AI providers (Ollama, Azure OpenAI).

### Architecture Overview

```
Test Execution → Agent Tests (.NET & Python) → Metrics JSON
                                                    ↓
                                        process_results_ollama.py
                                                    ↓
                                    Reports & Analysis (docs/)
```

## Agent-Specific Development Guidelines

### 1. Test Execution Workflow

When running performance tests, agents should:

```powershell
# Windows PowerShell
.\run_tests.ps1 -Iterations 1000 -TestMode standard -AgentType Ollama

# Linux/macOS
./run_tests.sh

# Windows Command Prompt
run_tests.bat
```

**Key Environment Variables:**

- `ITERATIONS` - Test repetitions (default: 1000)
- `TEST_MODE` - Test type: `standard`, `batch`, `concurrent`, `streaming`, `scenarios`
- `BATCH_SIZE` - Batch items (default: 10)
- `CONCURRENT_REQUESTS` - Parallel requests (default: 5)
- `OLLAMA_HOST` - Ollama endpoint (default: <http://localhost:11434>)
- `OLLAMA_MODEL_NAME` / `OLLAMA_CHAT_MODEL_ID` - Model name (default: ministral-3)

### 2. Building and Running Agents

#### .NET Agents

**Requirements:**

- .NET 10.0 SDK or later
- NuGet packages: `Microsoft.Agents.AI`, `OllamaSharp`, `System.Management`

**Build & Run:**

```powershell
cd dotnet/OllamaAgent
dotnet build
$env:ITERATIONS=100; dotnet run
```

#### Python Agents

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

**Environment file (.env):**

```
OLLAMA_HOST=http://localhost:11434
OLLAMA_CHAT_MODEL_ID=ministral-3
```

### 3. Metrics Collection Standards

**Required JSON Structure:**

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
    "OSSystem": "Windows|Linux|Darwin",
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

### 4. Agent Implementation Pattern

**Standard initialization sequence (both .NET and Python):**

1. **Load configuration** from environment variables with fallback defaults
2. **Execute warmup call** to prepare model/framework
3. **Run iteration loop** with precise timing
4. **Collect machine information** before export
5. **Export metrics** to JSON with timestamp

**Example (.NET):**

```csharp
var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL_NAME") ?? "ministral-3";
var agent = new OllamaApiClient(endpoint, model).CreateAIAgent(...);
// Warmup → Iterations → Export with MachineInfo
```

**Example (Python):**

```python
model = os.getenv("OLLAMA_CHAT_MODEL_ID", "ministral-3")
agent = OllamaChatClient(model_id=model).create_agent(...)
# Warmup → Iterations → Export with MachineInfo
```

## File Organization Rules

### Directory Structure

```
MAF-PerformanceComparison/
├── docs/                           # ✅ ALL DOCUMENTATION HERE
│   ├── MACHINE_INFO_GUIDE.md
│   ├── comparison_prompt_template.md
│   ├── IMPLEMENTATION_SUMMARY.md
│   ├── VERIFICATION_CHECKLIST.md
│   ├── DETAILED_GUIDE.md
│   ├── SCRIPTS_README.md
│   └── MACHINE_INFO_CHANGELOG.md
├── dotnet/                         # .NET agent implementations
│   ├── OllamaAgent/
│   ├── AzureOpenAIAgent/
│   └── HelloWorldAgent/
├── python/                         # Python agent implementations
│   ├── ollama_agent/
│   ├── azure_openai_agent/
│   └── hello_world_agent/
├── scenario-1-basic-metrics/      # Test scenarios
├── scenario-2-enhanced-metrics/
├── scenario-3-aspire-web/
├── tests_results/                 # Auto-generated test output
├── README.md                       # ✅ Root quick start only
├── .github/
│   ├── copilot-instructions.md    # ✅ GitHub Copilot guidance
│   └── agent-instructions.md      # ✅ This file
└── run_tests.*                     # Test orchestration scripts
```

### Naming Conventions

- **Metrics files:** `metrics_<language>_<provider>_<timestamp>.json`
  - Example: `metrics_dotnet_ollama_20260104_003410.json`
- **Reports:** `comparison_report_<mode>_<iter>iter.md`, `analysis_report_<mode>_<iter>iter.md`
- **Environments:** `<agent_type>_agent` (lowercase, underscore-separated)
- **Documentation:** Descriptive names in UPPER_CASE with underscores in `docs/`

## Agent Development Patterns

### Adding New Test Mode

1. Update `run_tests.ps1|sh|bat` with new `TestMode` case
2. Implement in both:
   - `dotnet/*/Program.cs` (C# implementation)
   - `python/*/main.py` (Python implementation)
3. Ensure timing logic is consistent between implementations
4. Export metrics maintaining JSON structure consistency
5. Update `process_results_ollama.py` for new report formatting
6. Document in `docs/comparison_prompt_template.md`

### Adding New Metrics

1. **Collect** metric in agent code before JSON export
2. **Add** to `Metrics` section (or create new top-level section if needed)
3. **Update** `process_results_ollama.py` markdown display logic
4. **Document** interpretation in `docs/MACHINE_INFO_GUIDE.md`
5. **Test** with both .NET and Python implementations

### Cross-Platform Compatibility

**Machine Information Collection:**

- Use platform detection to avoid OS-specific calls
- .NET: `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`
- Python: `platform.system() == "Windows"`
- Graceful fallback when system info unavailable

**Path Handling:**

- .NET: Use `Path.Combine()` for all path operations
- Python: Use `os.path.join()` for all path operations
- Never hardcode path separators (`/` or `\`)

**Shell Scripts:**

- Maintain PowerShell (`.ps1`) for Windows
- Maintain Bash (`.sh`) for Linux/macOS
- Keep batch (`.bat`) for Windows Command Prompt
- Synchronize functionality across all versions

## Integration Points

### External Services

**Ollama API** (Default Provider)

- Endpoint: `http://localhost:11434` (configurable)
- Requires local Ollama installation
- Models: Install via `ollama pull <model>`

**Azure OpenAI** (Optional)

- Configure via `.env` with Azure credentials
- Used in `AzureOpenAIAgent` implementations

### Key Libraries

**Python:**

- `agent-framework-ollama` - Microsoft Agent Framework (Ollama backend)
- `psutil` - System metrics (CPU, memory, GPU detection)
- `python-dotenv` - Environment variable management

**.NET:**

- `Microsoft.Agents.AI` - Core framework
- `OllamaSharp` - Ollama HTTP client
- `System.Management` - WMI for Windows system info (with fallback)

### Data Flow

1. **Test Execution** → Generates `metrics_*.json` in agent directory
2. **Result Processing** → `process_results_ollama.py` discovers and moves metrics
3. **Report Generation** → Creates comparison and analysis reports in `tests_results/`
4. **AI Analysis** → Ollama agent analyzes metrics for insights
5. **Output** → Timestamped folder with all results and reports

## Documentation References

**Primary Documentation (all in `docs/` folder):**

- [`docs/MACHINE_INFO_GUIDE.md`](../docs/MACHINE_INFO_GUIDE.md) - Metrics interpretation and machine information
- [`docs/comparison_prompt_template.md`](../docs/comparison_prompt_template.md) - LLM analysis structure and prompts
- [`docs/IMPLEMENTATION_SUMMARY.md`](../docs/IMPLEMENTATION_SUMMARY.md) - Architecture deep-dive and feature details
- [`docs/VERIFICATION_CHECKLIST.md`](../docs/VERIFICATION_CHECKLIST.md) - Implementation verification status
- [`docs/DETAILED_GUIDE.md`](../docs/DETAILED_GUIDE.md) - Comprehensive configuration and advanced usage
- [`docs/SCRIPTS_README.md`](../docs/SCRIPTS_README.md) - Test automation and script details
- [`docs/MACHINE_INFO_CHANGELOG.md`](../docs/MACHINE_INFO_CHANGELOG.md) - Machine info collection implementation

**Root-level References:**

- [`README.md`](../README.md) - Quick start guide
- [`.github/copilot-instructions.md`](copilot-instructions.md) - GitHub Copilot guidance
- [`.github/agent-instructions.md`](agent-instructions.md) - This file

## Agent Best Practices

### Code Quality

1. **Consistency:** Maintain parity between .NET and Python implementations
2. **Error Handling:** Include try-catch blocks with meaningful error messages
3. **Logging:** Use consistent logging patterns across agents
4. **Testing:** Verify cross-platform compatibility
5. **Documentation:** Update `docs/` folder when adding features

### Performance Considerations

1. **Warmup:** Always include warmup phase before timed iterations
2. **Timing:** Use high-precision timers (.NET: `Stopwatch`, Python: `time.perf_counter()`)
3. **Memory:** Track memory usage throughout test execution
4. **Resource Cleanup:** Properly dispose of resources and connections
5. **Async Operations:** Use async patterns where appropriate

### Metrics Accuracy

1. **Precision:** Use milliseconds with 3 decimal places for timing
2. **Consistency:** Ensure both implementations measure the same operations
3. **Completeness:** Capture all required metrics in JSON structure
4. **Machine Info:** Collect comprehensive system information
5. **Validation:** Verify metrics before export

## Quick Diagnostic Commands

```powershell
# Check Ollama service status
curl http://localhost:11434/api/tags

# List available Ollama models
ollama list

# Run a quick test (.NET)
cd dotnet/OllamaAgent
$env:ITERATIONS=3
dotnet run

# Run a quick test (Python)
cd python/ollama_agent
export ITERATIONS=3
python main.py

# Process results manually
python process_results_ollama.py

# View latest metrics (PowerShell)
Get-Content tests_results/*/metrics_*.json | ConvertFrom-Json | ConvertTo-Json -Depth 10

# View latest metrics (Bash)
cat tests_results/*/metrics_*.json | jq .
```

## Common Troubleshooting

### Ollama Connection Issues

- Verify Ollama is running: `ollama list`
- Check endpoint: Default is `http://localhost:11434`
- Ensure model is downloaded: `ollama pull ministral-3`

### Metrics Not Generated

- Check agent completed successfully (no exceptions)
- Verify write permissions in agent directory
- Look for error messages in console output

### Cross-Platform Issues

- Machine info: Use platform detection, graceful fallbacks
- Paths: Use OS-appropriate path combiners
- Line endings: Configure git to handle CRLF/LF automatically

### Memory Issues

- Large iteration counts may require increased heap size
- Monitor system resources during test runs
- Consider batch processing for very large tests

## Summary

As an AI agent working on this project:

1. **📁 Documentation Rule:** ALL documentation goes in `docs/` folder
2. **🔄 Consistency:** Maintain parity between .NET and Python implementations
3. **📊 Metrics:** Follow standard JSON structure for all test outputs
4. **🌐 Cross-Platform:** Test on Windows, Linux, and macOS
5. **📖 References:** Consult `docs/` folder for detailed guidance
6. **🛠️ Tools:** Use provided scripts for testing and result processing
7. **💡 Best Practices:** Follow established patterns for agent implementation

When in doubt, refer to existing implementations in `dotnet/` and `python/` folders, and consult the comprehensive guides in the `docs/` folder.
