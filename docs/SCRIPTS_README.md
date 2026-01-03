# Test Automation Scripts

This directory contains scripts to automate performance testing and result organization for the Microsoft Agent Framework comparison project.

## Available Scripts

### 1. Test Execution Scripts

#### Bash Script (Linux/Mac)
```bash
./run_tests.sh
```

#### PowerShell Script (Windows)
```powershell
.\run_tests.ps1
```

#### Batch Script (Windows)
```batch
run_tests.bat
```

### 2. Results Organization Script

```bash
python3 organize_results.py
```

## Usage Examples

### Basic Usage

Run tests with default settings (standard mode, 1000 iterations, HelloWorld agent):

```bash
# Linux/Mac
./run_tests.sh

# Windows PowerShell
.\run_tests.ps1

# Windows Command Prompt
run_tests.bat
```

### Customize Test Mode

Set the `TEST_MODE` environment variable to run different test modes:

```bash
# Standard mode (default)
TEST_MODE=standard ./run_tests.sh

# Batch processing mode
TEST_MODE=batch BATCH_SIZE=20 ./run_tests.sh

# Concurrent requests mode
TEST_MODE=concurrent CONCURRENT_REQUESTS=10 ./run_tests.sh

# Streaming mode
TEST_MODE=streaming ./run_tests.sh

# Scenarios mode (tests multiple prompt types)
TEST_MODE=scenarios ./run_tests.sh
```

PowerShell examples:
```powershell
# Batch mode
.\run_tests.ps1 -TestMode batch -BatchSize 20

# Concurrent mode
.\run_tests.ps1 -TestMode concurrent -ConcurrentRequests 10
```

### Select Agent Type

Use the `AGENT_TYPE` environment variable to choose which agent to test:

```bash
# Test HelloWorld agent only
AGENT_TYPE=HelloWorld ./run_tests.sh

# Test AzureOpenAI agent only (requires .env configuration)
AGENT_TYPE=AzureOpenAI ./run_tests.sh

# Test Ollama agent only (requires Ollama installation)
AGENT_TYPE=Ollama ./run_tests.sh

# Test all available agents
AGENT_TYPE=All ./run_tests.sh
```

PowerShell example:
```powershell
.\run_tests.ps1 -AgentType All
```

### Customize Iteration Count

```bash
# Run 500 iterations
ITERATIONS=500 ./run_tests.sh

# Run 2000 iterations for more statistical significance
ITERATIONS=2000 ./run_tests.sh
```

PowerShell example:
```powershell
.\run_tests.ps1 -Iterations 2000
```

### Combined Example

Run all agents in batch mode with 100 iterations and batch size of 15:

```bash
TEST_MODE=batch BATCH_SIZE=15 ITERATIONS=100 AGENT_TYPE=All ./run_tests.sh
```

PowerShell:
```powershell
.\run_tests.ps1 -TestMode batch -BatchSize 15 -Iterations 100 -AgentType All
```

## Organizing Test Results

After running tests, use the `organize_results.py` script to:
1. Create a timestamped folder in `tests_results/`
2. Move all metrics JSON files to the new folder
3. Generate a comparison markdown file ready for LLM analysis
4. **Automatically analyze the results using Ollama or Azure OpenAI** (if configured)

```bash
python3 organize_results.py
```

### Requirements

Install the required dependencies:

```bash
pip install -r requirements.txt
```

### LLM Analysis Feature

The script automatically detects and uses available LLM providers to analyze test results:

**Ollama (Local)**
- The script checks if Ollama is running on `http://localhost:11434` (or `OLLAMA_ENDPOINT` if set)
- Uses the model specified in `OLLAMA_MODEL_NAME` environment variable (default: `llama2`)
- No authentication required

**Azure OpenAI (Cloud)**
- Requires `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_DEPLOYMENT_NAME` environment variables
- Uses Azure CLI authentication (`az login` must be run first)
- Recommended for production use

**Configuration**

Set environment variables in a `.env` file or shell:

```bash
# For Ollama
export OLLAMA_ENDPOINT=http://localhost:11434
export OLLAMA_MODEL_NAME=llama2

# For Azure OpenAI
export AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
export AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini
```

**Output Files**

The script will generate:
- `comparison_report.md` - Detailed metrics comparison with LLM-ready prompts
- `analysis_report.md` - **Automated analysis from the LLM** (if provider is available)

The script will:
- Find all `metrics_*.json` files in the current directory and subdirectories
- Create a folder named `YYYYMMDD_HHMMSS_{test_mode}` in `tests_results/`
- Move all metrics files to the new folder
- Generate `comparison_report.md` with LLM-ready prompts for analysis

## Workflow Example

Complete workflow for running tests and analyzing results:

```bash
# 1. Run tests in standard mode
TEST_MODE=standard ITERATIONS=1000 AGENT_TYPE=HelloWorld ./run_tests.sh

# 2. Organize results
python3 organize_results.py

# 3. Review the comparison report
cat tests_results/[generated_folder]/comparison_report.md

# 4. Copy the LLM prompts and paste into your AI assistant for analysis
```

## Test Modes Reference

| Mode | Description | Additional Parameters |
|------|-------------|----------------------|
| `standard` | Sequential execution (default) | - |
| `batch` | Batch processing | `BATCH_SIZE` (default: 10) |
| `concurrent` | Concurrent requests | `CONCURRENT_REQUESTS` (default: 5) |
| `streaming` | Streaming responses with TTFT | - |
| `scenarios` | Multiple prompt types | - |

## Agent Types Reference

| Agent Type | Description | Requirements |
|------------|-------------|--------------|
| `HelloWorld` | Demo/mock agent | None |
| `AzureOpenAI` | Azure OpenAI integration | `.env` file with credentials |
| `Ollama` | Local Ollama integration | Ollama installed and running |
| `All` | Run all available agents | Optional for AzureOpenAI and Ollama |

## Environment Variables

### For Test Scripts

- `TEST_MODE`: Test execution mode (standard, batch, concurrent, streaming, scenarios)
- `ITERATIONS`: Number of test iterations (default: 1000)
- `AGENT_TYPE`: Agent to test (HelloWorld, AzureOpenAI, Ollama, All)
- `BATCH_SIZE`: Batch size for batch mode (default: 10)
- `CONCURRENT_REQUESTS`: Concurrent requests for concurrent mode (default: 5)

### For Agent Configuration

See individual agent directories for specific configuration requirements:
- Azure OpenAI: Requires `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_DEPLOYMENT_NAME`
- Ollama: Optional `OLLAMA_ENDPOINT` and `OLLAMA_MODEL_NAME`

## Troubleshooting

### Python Dependencies

If Python tests fail with import errors, install dependencies:

```bash
# For HelloWorld agent
cd python/hello_world_agent
pip install -r requirements.txt

# For all agents
for dir in python/*/; do
    if [ -f "$dir/requirements.txt" ]; then
        cd "$dir"
        pip install -r requirements.txt
        cd ../..
    fi
done
```

### .NET Build Issues

If .NET tests fail to build:

```bash
# Restore NuGet packages
cd dotnet/HelloWorldAgent
dotnet restore
dotnet build
```

### Script Execution Permissions

If you get permission errors on Linux/Mac:

```bash
chmod +x run_tests.sh organize_results.py
```

## Output Files

### Metrics Files

Format: `metrics_{language}_{provider}_{testmode}_{timestamp}.json`

Examples:
- `metrics_dotnet_helloworld_standard_20260103_123456.json`
- `metrics_python_ollama_batch_20260103_123456.json`

### Organized Results

After running `organize_results.py`:
```
tests_results/
└── 20260103_123456_standard/
    ├── metrics_dotnet_helloworld_standard_20260103_123456.json
    ├── metrics_python_helloworld_standard_20260103_123456.json
    └── comparison_report.md
```

## Next Steps

After organizing results:
1. Open `comparison_report.md` in the generated folder
2. Copy the LLM comparison prompts
3. Paste into your preferred AI assistant (ChatGPT, Claude, Copilot, etc.)
4. Review the performance analysis and insights
