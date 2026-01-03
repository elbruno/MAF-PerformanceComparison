param(
    [string]$TestMode = "standard",
    [int]$Iterations = 10,
    [string]$AgentType = "Ollama",
    [int]$BatchSize = 10,
    [int]$ConcurrentRequests = 5
)

$ErrorActionPreference = 'Stop'

function WriteColor {
    param([string]$msg, [string]$color = 'White')
    try { $c = [System.Enum]::Parse([System.ConsoleColor], $color) } catch { $c = [System.ConsoleColor]::White }
    Write-Host $msg -ForegroundColor $c
}

WriteColor '============================================' 'Cyan'
WriteColor 'Microsoft Agent Framework Performance Tests' 'Cyan'
WriteColor '============================================' 'Cyan'
Write-Host ''
WriteColor 'Configuration:' 'Green'
Write-Host "  Agent Type: $AgentType"
Write-Host "  Test Mode: $TestMode"
Write-Host "  Iterations: $Iterations"
if ($TestMode -eq "batch") {
    Write-Host "  Batch Size: $BatchSize"
}
if ($TestMode -eq "concurrent") {
    Write-Host "  Concurrent Requests: $ConcurrentRequests"
}
Write-Host ''

function RunDotNet {
    param([string]$agentDir, [string]$agentName)
    if (-not (Test-Path $agentDir)) {
        WriteColor "Skipping .NET $agentName: directory not found: $agentDir" 'Yellow'
        return
    }
    WriteColor "Running .NET $agentName test..." 'Yellow'
    Push-Location $agentDir
    try {
        $env:TEST_MODE = $TestMode
        $env:ITERATIONS = $Iterations
        $env:BATCH_SIZE = $BatchSize
        $env:CONCURRENT_REQUESTS = $ConcurrentRequests

        dotnet build | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Failed to build .NET project' }
        dotnet run
        WriteColor "[OK] .NET $agentName test completed" 'Green'
        Write-Host ''
    }
    catch {
        WriteColor "Error running .NET test: $_" 'Red'
    }
    finally {
        Pop-Location
    }
}

function RunPython {
    param([string]$agentDir, [string]$agentName)
    if (-not (Test-Path $agentDir)) {
        WriteColor "Skipping Python $agentName: directory not found: $agentDir" 'Yellow'
        return
    }
    WriteColor "Running Python $agentName test..." 'Yellow'
    Push-Location $agentDir
    try {
        $env:TEST_MODE = $TestMode
        $env:ITERATIONS = $Iterations
        $env:BATCH_SIZE = $BatchSize
        $env:CONCURRENT_REQUESTS = $ConcurrentRequests

        $python = $null
        if (Get-Command py -ErrorAction SilentlyContinue) { $python = 'py' }
        elseif (Get-Command python -ErrorAction SilentlyContinue) { $python = 'python' }
        elseif (Get-Command python3 -ErrorAction SilentlyContinue) { $python = 'python3' }

        if (-not $python) {
            Write-Host 'Python not found on PATH. To run Python tests, install Python and ensure it is on PATH.' -ForegroundColor Yellow
            Write-Host "Skipping Python $agentName test." -ForegroundColor Yellow
            return
        }

        & $python main.py
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Python test failed with exit code $LASTEXITCODE" -ForegroundColor Yellow
            $req = Join-Path $agentDir 'requirements.txt'
            if (Test-Path $req) {
                Write-Host "To install requirements run: $python -m pip install -r `"$req`"" -ForegroundColor Yellow
            }
            return
        }

        WriteColor "[OK] Python $agentName test completed" 'Green'
        Write-Host ''
    }
    catch {
        WriteColor "Error running Python test: $_" 'Red'
    }
    finally {
        Pop-Location
    }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

switch ($AgentType) {
    'HelloWorld' {
        RunDotNet (Join-Path $scriptDir 'dotnet\HelloWorldAgent') 'HelloWorld'
        RunPython (Join-Path $scriptDir 'python\hello_world_agent') 'HelloWorld'
    }
    'AzureOpenAI' {
        RunDotNet (Join-Path $scriptDir 'dotnet\AzureOpenAIAgent') 'AzureOpenAI'
        RunPython (Join-Path $scriptDir 'python\azure_openai_agent') 'AzureOpenAI'
    }
    'Ollama' {
        RunDotNet (Join-Path $scriptDir 'dotnet\OllamaAgent') 'Ollama'
        RunPython (Join-Path $scriptDir 'python\ollama_agent') 'Ollama'
    }
    'All' {
        WriteColor 'Running all agent types...' 'Cyan'
        RunDotNet (Join-Path $scriptDir 'dotnet\HelloWorldAgent') 'HelloWorld'
        RunPython (Join-Path $scriptDir 'python\hello_world_agent') 'HelloWorld'
        RunDotNet (Join-Path $scriptDir 'dotnet\AzureOpenAIAgent') 'AzureOpenAI'
        RunPython (Join-Path $scriptDir 'python\azure_openai_agent') 'AzureOpenAI'
        RunDotNet (Join-Path $scriptDir 'dotnet\OllamaAgent') 'Ollama'
        RunPython (Join-Path $scriptDir 'python\ollama_agent') 'Ollama'
    }
    default {
        WriteColor "Invalid agent type: $AgentType" 'Red'
        exit 1
    }
}

WriteColor '============================================' 'Green'
WriteColor 'All tests completed successfully!' 'Green'
WriteColor '============================================' 'Green'
Write-Host ''
WriteColor 'Next steps:' 'Cyan'
Write-Host '1. Run: python organize_results.py'
Write-Host '2. Check the tests_results\ folder for organized results'
