@echo off
REM Run performance tests for Microsoft Agent Framework implementations
REM This script runs tests in different modes for both .NET and Python implementations

setlocal enabledelayedexpansion

REM Default values
if "%TEST_MODE%"=="" set TEST_MODE=standard
if "%ITERATIONS%"=="" set ITERATIONS=10
if "%AGENT_TYPE%"=="" set AGENT_TYPE=Ollama
if "%BATCH_SIZE%"=="" set BATCH_SIZE=10
if "%CONCURRENT_REQUESTS%"=="" set CONCURRENT_REQUESTS=5

echo ============================================
echo Microsoft Agent Framework Performance Tests
echo ============================================
echo.
echo Configuration:
echo   Agent Type: %AGENT_TYPE%
echo   Test Mode: %TEST_MODE%
echo   Iterations: %ITERATIONS%
if "%TEST_MODE%"=="batch" echo   Batch Size: %BATCH_SIZE%
if "%TEST_MODE%"=="concurrent" echo   Concurrent Requests: %CONCURRENT_REQUESTS%
echo.

REM Get the script directory
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

REM Clean up old metrics files before running tests
echo Cleaning up old metrics files...
for /r "%SCRIPT_DIR%dotnet" %%f in (metrics_*.json) do (
    if not "%%~dpf" == "%SCRIPT_DIR%tests_results\" (
        del "%%f" 2>nul && echo   Deleted: %%~nxf
    )
)
for /r "%SCRIPT_DIR%python" %%f in (metrics_*.json) do (
    if not "%%~dpf" == "%SCRIPT_DIR%tests_results\" (
        del "%%f" 2>nul && echo   Deleted: %%~nxf
    )
)
echo.

REM Run tests based on agent type
if /i "%AGENT_TYPE%"=="HelloWorld" (
    call :run_dotnet_test "%SCRIPT_DIR%dotnet\HelloWorldAgent" "HelloWorld"
    call :run_python_test "%SCRIPT_DIR%python\hello_world_agent" "HelloWorld"
    goto :done
)

if /i "%AGENT_TYPE%"=="AzureOpenAI" (
    call :run_dotnet_test "%SCRIPT_DIR%dotnet\AzureOpenAIAgent" "AzureOpenAI"
    call :run_python_test "%SCRIPT_DIR%python\azure_openai_agent" "AzureOpenAI"
    goto :done
)

if /i "%AGENT_TYPE%"=="Ollama" (
    call :run_dotnet_test "%SCRIPT_DIR%dotnet\OllamaAgent" "Ollama"
    call :run_python_test "%SCRIPT_DIR%python\ollama_agent" "Ollama"
    goto :done
)

if /i "%AGENT_TYPE%"=="All" (
    echo Running all agent types...
    echo.
    
    call :run_dotnet_test "%SCRIPT_DIR%dotnet\HelloWorldAgent" "HelloWorld"
    call :run_python_test "%SCRIPT_DIR%python\hello_world_agent" "HelloWorld"
    
    REM Check if .env files exist for AzureOpenAI
    if exist "%SCRIPT_DIR%dotnet\AzureOpenAIAgent\.env" (
        call :run_dotnet_test "%SCRIPT_DIR%dotnet\AzureOpenAIAgent" "AzureOpenAI"
        call :run_python_test "%SCRIPT_DIR%python\azure_openai_agent" "AzureOpenAI"
    ) else (
        echo Skipping AzureOpenAI tests (no .env configuration found)
    )
    
    REM Check if Ollama is available
    where ollama >nul 2>&1
    if %ERRORLEVEL%==0 (
        call :run_dotnet_test "%SCRIPT_DIR%dotnet\OllamaAgent" "Ollama"
        call :run_python_test "%SCRIPT_DIR%python\ollama_agent" "Ollama"
    ) else (
        echo Skipping Ollama tests (Ollama not installed)
    )
    goto :done
)

echo Invalid agent type: %AGENT_TYPE%
echo Valid options: HelloWorld, AzureOpenAI, Ollama, All
exit /b 1

:done
echo ============================================
echo All tests completed successfully!
echo ============================================
echo.
echo Processing results...
cd /d "%SCRIPT_DIR%"

REM Run process_results_ollama.py automatically
where python3 >nul 2>&1
if %ERRORLEVEL%==0 (
    python3 process_results_ollama.py
    if %ERRORLEVEL%==0 (
        echo + Results processed successfully
        echo.
        echo Check the tests_results\ folder for organized results
    ) else (
        echo Failed to process results
    )
) else (
    where py >nul 2>&1
    if %ERRORLEVEL%==0 (
        py process_results_ollama.py
        if %ERRORLEVEL%==0 (
            echo + Results processed successfully
            echo.
            echo Check the tests_results\ folder for organized results
        ) else (
            echo Failed to process results
        )
    ) else (
        where python >nul 2>&1
        if %ERRORLEVEL%==0 (
            python process_results_ollama.py
            if %ERRORLEVEL%==0 (
                echo + Results processed successfully
                echo.
                echo Check the tests_results\ folder for organized results
            ) else (
                echo Failed to process results
            )
        ) else (
            echo Python not found. Skipping results processing.
            echo Next steps:
            echo 1. Run: python process_results_ollama.py
            echo 2. Check the tests_results\ folder for organized results
        )
    )
)
goto :eof

:run_dotnet_test
set agent_dir=%~1
set agent_name=%~2

echo Running .NET %agent_name% test...
cd /d "%agent_dir%"

dotnet build >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo Failed to build .NET project
    exit /b 1
)

dotnet run
echo + .NET %agent_name% test completed
echo.

cd /d "%SCRIPT_DIR%"
exit /b 0

:run_python_test
set agent_dir=%~1
set agent_name=%~2

echo Running Python %agent_name% test...
cd /d "%agent_dir%"

REM Try python3 first, then py, then python
where python3 >nul 2>&1
if %ERRORLEVEL%==0 (
    python3 main.py
) else (
    where py >nul 2>&1
    if %ERRORLEVEL%==0 (
        py main.py
    ) else (
        python main.py
    )
)

echo + Python %agent_name% test completed
echo.

cd /d "%SCRIPT_DIR%"
exit /b 0
