# Run performance tests for Microsoft Agent Framework implementations
# This script runs tests in different modes for both .NET and Python implementations

param(
    [string]$TestMode = "standard",
    [int]$Iterations = 1000,
    [string]$AgentType = "HelloWorld",
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
function Run-DotNetTest {
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
function Run-PythonTest {
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
        
        # Run Python test
        python main.py
        
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
        Run-DotNetTest "$ScriptDir\dotnet\HelloWorldAgent" "HelloWorld"
        Run-PythonTest "$ScriptDir\python\hello_world_agent" "HelloWorld"
    }
    "AzureOpenAI" {
        Run-DotNetTest "$ScriptDir\dotnet\AzureOpenAIAgent" "AzureOpenAI"
        Run-PythonTest "$ScriptDir\python\azure_openai_agent" "AzureOpenAI"
    }
    "Ollama" {
        Run-DotNetTest "$ScriptDir\dotnet\OllamaAgent" "Ollama"
        Run-PythonTest "$ScriptDir\python\ollama_agent" "Ollama"
    }
    "All" {
        Write-ColorOutput "Running all agent types..." "Cyan"
        Write-Host ""
        
        Run-DotNetTest "$ScriptDir\dotnet\HelloWorldAgent" "HelloWorld"
        Run-PythonTest "$ScriptDir\python\hello_world_agent" "HelloWorld"
        
        # Check if .env files exist for AzureOpenAI
        if ((Test-Path "$ScriptDir\dotnet\AzureOpenAIAgent\.env") -or (Test-Path "$ScriptDir\python\azure_openai_agent\.env")) {
            try {
                Run-DotNetTest "$ScriptDir\dotnet\AzureOpenAIAgent" "AzureOpenAI"
                Run-PythonTest "$ScriptDir\python\azure_openai_agent" "AzureOpenAI"
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
                Run-DotNetTest "$ScriptDir\dotnet\OllamaAgent" "Ollama"
                Run-PythonTest "$ScriptDir\python\ollama_agent" "Ollama"
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
Write-Host "1. Run: python organize_results.py"
Write-Host "2. Check the tests_results\ folder for organized results"
