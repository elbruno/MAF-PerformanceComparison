# Run performance tests for Microsoft Agent Framework implementations
# This script runs tests in different modes for both .NET and Python implementations

param(
    [string]$TestMode = "standard",
    [int]$Iterations = 10,
    [string]$AgentType = "Ollama",
    [int]$BatchSize = 10,
    [int]$ConcurrentRequests = 5
)

$ErrorActionPreference = "Stop"

# Color output helper
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "============================================" "Cyan"
Write-ColorOutput "Microsoft Agent Framework Performance Tests" "Cyan"
Write-ColorOutput "============================================" "Cyan"
Write-Host ""
Write-ColorOutput "Configuration:" "Green"
Write-Host "  Agent Type: $AgentType"
Write-Host "  Test Mode: $TestMode"
Write-Host "  Iterations: $Iterations"
if ($TestMode -eq "batch") {
    Write-Host "  Batch Size: $BatchSize"
}
if ($TestMode -eq "concurrent") {
    Write-Host "  Concurrent Requests: $ConcurrentRequests"
}
Write-Host ""

# Function to run .NET tests
function Invoke-DotNetTest {
    param(
        [string]$AgentDir,
        [string]$AgentName
    )
    
    Write-ColorOutput "Running .NET $AgentName test..." "Yellow"
    Push-Location $AgentDir
    
    try {
        # Set environment variables
        $env:TEST_MODE = $TestMode
        $env:ITERATIONS = $Iterations
        $env:BATCH_SIZE = $BatchSize
        $env:CONCURRENT_REQUESTS = $ConcurrentRequests
        
        # Build and run
        dotnet build | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build .NET project"
        }
        dotnet run
        
        Write-ColorOutput "✓ .NET $AgentName test completed" "Green"
        Write-Host ""
    }
    catch {
        Write-ColorOutput "Error running .NET test: $_" "Red"
        throw
    }
    finally {
        Pop-Location
    }
}

# Function to run Python tests
function Invoke-PythonTest {
    param(
        [string]$AgentDir,
        [string]$AgentName
    )
    
    Write-ColorOutput "Running Python $AgentName test..." "Yellow"
    Push-Location $AgentDir
    
    try {
        # Set environment variables
        $env:TEST_MODE = $TestMode
        $env:ITERATIONS = $Iterations
        $env:BATCH_SIZE = $BatchSize
        $env:CONCURRENT_REQUESTS = $ConcurrentRequests
        
        # Find a Python launcher/executable. Prefer the Windows 'py' launcher,
        # then 'python', then 'python3'. If none found, skip the Python test with
        # a helpful message rather than invoking a missing binary (which on
        # Windows shows the Store prompt).
        function Find-PythonExe {
            if (Get-Command py -ErrorAction SilentlyContinue) { return "py" }
            if (Get-Command python -ErrorAction SilentlyContinue) { return "python" }
            if (Get-Command python3 -ErrorAction SilentlyContinue) { return "python3" }
            return $null
        }

        $pythonExe = Find-PythonExe
        if (-not $pythonExe) {
            Write-ColorOutput "Python not found on PATH. Install Python or enable App Execution Aliases (Settings `> Apps `> App execution aliases)." "Yellow"
            Write-ColorOutput "Skipping Python $AgentName test." "Yellow"
            Pop-Location
            return
        }

        # Invoke the discovered Python executable. Use the call operator (&) so
        # PowerShell runs the program correctly. Check the exit code and treat
        # a non-zero exit as an error.
        & $pythonExe main.py
        if ($LASTEXITCODE -ne 0) {
            throw "Python test exited with code $LASTEXITCODE"
        }
        
        Write-ColorOutput "✓ Python $AgentName test completed" "Green"
        Write-Host ""
    }
    catch {
        Write-ColorOutput "Error running Python test: $_" "Red"
        throw
    }
    finally {
        Pop-Location
    }
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Run tests based on agent type
switch ($AgentType) {
    "HelloWorld" {
        Invoke-DotNetTest "$ScriptDir\dotnet\HelloWorldAgent" "HelloWorld"
        Invoke-PythonTest "$ScriptDir\python\hello_world_agent" "HelloWorld"
    }
    "AzureOpenAI" {
        Invoke-DotNetTest "$ScriptDir\dotnet\AzureOpenAIAgent" "AzureOpenAI"
        Invoke-PythonTest "$ScriptDir\python\azure_openai_agent" "AzureOpenAI"
    }
    "Ollama" {
        Invoke-DotNetTest "$ScriptDir\dotnet\OllamaAgent" "Ollama"
        Invoke-PythonTest "$ScriptDir\python\ollama_agent" "Ollama"
    }
    "All" {
        Write-ColorOutput "Running all agent types..." "Cyan"
        Write-Host ""
        
        Invoke-DotNetTest "$ScriptDir\dotnet\HelloWorldAgent" "HelloWorld"
        Invoke-PythonTest "$ScriptDir\python\hello_world_agent" "HelloWorld"
        
        # Check if .env files exist for AzureOpenAI
        if ((Test-Path "$ScriptDir\dotnet\AzureOpenAIAgent\.env") -or (Test-Path "$ScriptDir\python\azure_openai_agent\.env")) {
            try {
                Invoke-DotNetTest "$ScriptDir\dotnet\AzureOpenAIAgent" "AzureOpenAI"
                Invoke-PythonTest "$ScriptDir\python\azure_openai_agent" "AzureOpenAI"
            }
            catch {
                Write-ColorOutput "Skipping AzureOpenAI tests (configuration error)" "Yellow"
            }
        }
        else {
            Write-ColorOutput "Skipping AzureOpenAI tests (no .env configuration found)" "Yellow"
        }
        
        # Check if Ollama is available
        if (Get-Command ollama -ErrorAction SilentlyContinue) {
            try {
                Invoke-DotNetTest "$ScriptDir\dotnet\OllamaAgent" "Ollama"
                Invoke-PythonTest "$ScriptDir\python\ollama_agent" "Ollama"
            }
            catch {
                Write-ColorOutput "Skipping Ollama tests (Ollama not available)" "Yellow"
            }
        }
        else {
            Write-ColorOutput "Skipping Ollama tests (Ollama not installed)" "Yellow"
        }
    }
    default {
        Write-ColorOutput "Invalid agent type: $AgentType" "Red"
        Write-Host "Valid options: HelloWorld, AzureOpenAI, Ollama, All"
        exit 1
    }
}

Write-ColorOutput "============================================" "Green"
Write-ColorOutput "All tests completed successfully!" "Green"
Write-ColorOutput "============================================" "Green"
Write-Host ""
Write-ColorOutput "Next steps:" "Cyan"
if (Get-Command python3 -ErrorAction SilentlyContinue) {
    Write-Host "1. Run: python3 organize_results.py"
}
elseif (Get-Command py -ErrorAction SilentlyContinue) {
    Write-Host "1. Run: py organize_results.py"
}
else {
    Write-Host "1. Run: python organize_results.py"
}
Write-Host "2. Check the tests_results\ folder for organized results"
