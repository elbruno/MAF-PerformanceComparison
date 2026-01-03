#!/bin/bash
# Run performance tests for Microsoft Agent Framework implementations
# This script runs tests in different modes for both .NET and Python implementations

set -e  # Exit on error

# Color codes for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Default values
TEST_MODE="${TEST_MODE:-standard}"
ITERATIONS="${ITERATIONS:-10}"
AGENT_TYPE="${AGENT_TYPE:-Ollama}"
BATCH_SIZE="${BATCH_SIZE:-10}"
CONCURRENT_REQUESTS="${CONCURRENT_REQUESTS:-5}"

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}Microsoft Agent Framework Performance Tests${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""
echo -e "${GREEN}Configuration:${NC}"
echo -e "  Agent Type: ${AGENT_TYPE}"
echo -e "  Test Mode: ${TEST_MODE}"
echo -e "  Iterations: ${ITERATIONS}"
if [ "$TEST_MODE" = "batch" ]; then
    echo -e "  Batch Size: ${BATCH_SIZE}"
fi
if [ "$TEST_MODE" = "concurrent" ]; then
    echo -e "  Concurrent Requests: ${CONCURRENT_REQUESTS}"
fi
echo ""

# Function to run .NET tests
run_dotnet_test() {
    local agent_dir=$1
    local agent_name=$2
    
    echo -e "${YELLOW}Running .NET ${agent_name} test...${NC}"
    cd "${agent_dir}"
    
    # Export environment variables for the test
    export TEST_MODE="${TEST_MODE}"
    export ITERATIONS="${ITERATIONS}"
    export BATCH_SIZE="${BATCH_SIZE}"
    export CONCURRENT_REQUESTS="${CONCURRENT_REQUESTS}"
    
    # Build and run
    dotnet build > /dev/null 2>&1 || { echo -e "${RED}Failed to build .NET project${NC}"; return 1; }
    dotnet run
    
    echo -e "${GREEN}✓ .NET ${agent_name} test completed${NC}"
    echo ""
    
    cd - > /dev/null
}

# Function to run Python tests
run_python_test() {
    local agent_dir=$1
    local agent_name=$2
    
    echo -e "${YELLOW}Running Python ${agent_name} test...${NC}"
    cd "${agent_dir}"
    
    # Export environment variables for the test
    export TEST_MODE="${TEST_MODE}"
    export ITERATIONS="${ITERATIONS}"
    export BATCH_SIZE="${BATCH_SIZE}"
    export CONCURRENT_REQUESTS="${CONCURRENT_REQUESTS}"
    
    # Run Python test
    python3 main.py
    
    echo -e "${GREEN}✓ Python ${agent_name} test completed${NC}"
    echo ""
    
    cd - > /dev/null
}

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

# Run tests based on agent type
case "${AGENT_TYPE}" in
    "HelloWorld")
        run_dotnet_test "${SCRIPT_DIR}/dotnet/HelloWorldAgent" "HelloWorld"
        run_python_test "${SCRIPT_DIR}/python/hello_world_agent" "HelloWorld"
        ;;
    "AzureOpenAI")
        run_dotnet_test "${SCRIPT_DIR}/dotnet/AzureOpenAIAgent" "AzureOpenAI"
        run_python_test "${SCRIPT_DIR}/python/azure_openai_agent" "AzureOpenAI"
        ;;
    "Ollama")
        run_dotnet_test "${SCRIPT_DIR}/dotnet/OllamaAgent" "Ollama"
        run_python_test "${SCRIPT_DIR}/python/ollama_agent" "Ollama"
        ;;
    "All")
        echo -e "${BLUE}Running all agent types...${NC}"
        echo ""
        run_dotnet_test "${SCRIPT_DIR}/dotnet/HelloWorldAgent" "HelloWorld"
        run_python_test "${SCRIPT_DIR}/python/hello_world_agent" "HelloWorld"
        
        # Check if .env files exist for AzureOpenAI
        if [ -f "${SCRIPT_DIR}/dotnet/AzureOpenAIAgent/.env" ] || [ -f "${SCRIPT_DIR}/python/azure_openai_agent/.env" ]; then
            run_dotnet_test "${SCRIPT_DIR}/dotnet/AzureOpenAIAgent" "AzureOpenAI" || true
            run_python_test "${SCRIPT_DIR}/python/azure_openai_agent" "AzureOpenAI" || true
        else
            echo -e "${YELLOW}Skipping AzureOpenAI tests (no .env configuration found)${NC}"
        fi
        
        # Check if Ollama is available
        if command -v ollama &> /dev/null; then
            run_dotnet_test "${SCRIPT_DIR}/dotnet/OllamaAgent" "Ollama" || true
            run_python_test "${SCRIPT_DIR}/python/ollama_agent" "Ollama" || true
        else
            echo -e "${YELLOW}Skipping Ollama tests (Ollama not installed)${NC}"
        fi
        ;;
    *)
        echo -e "${RED}Invalid agent type: ${AGENT_TYPE}${NC}"
        echo "Valid options: HelloWorld, AzureOpenAI, Ollama, All"
        exit 1
        ;;
esac

echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}All tests completed successfully!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo -e "${BLUE}Next steps:${NC}"
echo "1. Run: python3 organize_results.py"
echo "2. Check the tests_results/ folder for organized results"
