import asyncio
import json
import os
import time
import psutil
from datetime import datetime, timezone

from agent_framework.ollama import OllamaChatClient
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

"""
Ollama Agent Performance Test

This sample demonstrates implementing an Ollama agent with performance testing.

Ensure to install Ollama and have a model running locally before running the sample.
Not all Models support function calling, to test function calling try llama3.2 or qwen3:4b
Set the model to use via the OLLAMA_MODEL_NAME environment variable or modify the code below.
https://ollama.com/
"""


def get_time(location: str) -> str:
    """Get the current time."""
    return f"The current time in {location} is {datetime.now().strftime('%I:%M %p')}."


async def run_performance_test() -> None:
    """Run 1000 iterations of agent operations for performance testing."""
    print("=== Python Microsoft Agent Framework - Ollama Agent ===\n")
    
    # Configuration - Read from environment variables or use defaults
    endpoint = os.getenv("OLLAMA_ENDPOINT", "http://localhost:11434")
    model_name = os.getenv("OLLAMA_MODEL_NAME", "ministral-3")
    
    print(f"Configuring for Ollama endpoint: {endpoint}")
    print(f"Using model: {model_name}")
    print("\nNote: This requires Ollama to be running locally.")
    print("Install Ollama from: https://ollama.ai/")
    print(f"Start Ollama and pull the model: ollama pull {model_name}\n")
    
    # Start performance measurement
    start_time = time.time()
    process = psutil.Process(os.getpid())
    start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    
    # Performance test: Run agent operations. Make configurable via environment variable for easier testing.
    ITERATIONS = int(os.getenv("ITERATIONS", "1000"))
    iteration_times = []
    warmup_successful = False
    
    try:
        # Create agent using agent-framework with Ollama
        agent = OllamaChatClient(model_id=model_name).create_agent(
            name="PerformanceTestAgent",
            instructions="You are a helpful assistant. Provide brief, concise responses.",
            tools=get_time,
        )
        
        print("✓ Agent framework initialized successfully")
        print("✓ Ollama service configured")
        
        # Warmup call - prepares the model for subsequent calls
        print("⏳ Performing warmup call to prepare the model...")
        try:
            warmup_start = time.time()
            await agent.run("Hello, this is a warmup call.")
            warmup_end = time.time()
            warmup_time_ms = (warmup_end - warmup_start) * 1000
            print(f"✓ Warmup completed in {warmup_time_ms:.3f} ms")
            warmup_successful = True
        except Exception as warmup_ex:
            print(f"⚠ Warmup call failed: {warmup_ex}")
            print("Continuing with performance test...")
        
        print(f"✓ Running {ITERATIONS} iterations for performance testing\n")
        
        try:
            # Run 1000 iterations with actual Ollama calls
            for i in range(ITERATIONS):
                iteration_start = time.time()
                
                # Invoke the agent
                result = await agent.run(f"Say hello {i + 1}")
                
                iteration_end = time.time()
                iteration_times.append((iteration_end - iteration_start) * 1000)
                
                if (i + 1) % 100 == 0:
                    print(f"  Progress: {i + 1}/{ITERATIONS} iterations completed")
            
            # Show a sample streaming response
            print("\n--- Sample Agent Streaming Response ---")
            print("Agent: ", end="", flush=True)
            async for chunk in agent.run_stream("What time is it in Seattle?"):
                if chunk.text:
                    print(chunk.text, end="", flush=True)
            print("\n---------------------------\n")
            
        except Exception as connect_ex:
            print(f"\n⚠ Could not connect to Ollama at {endpoint}")
            print("Please ensure Ollama is running and the model is available.")
            print(f"Error: {connect_ex}")
            print("\nRunning in demo mode instead...\n")
            
            # Run in demo mode if Ollama is not available
            for i in range(ITERATIONS):
                iteration_start = time.time()
                
                # Simulate agent operation
                response = f"Response {i + 1} (Demo mode)"
                
                iteration_end = time.time()
                iteration_times.append((iteration_end - iteration_start) * 1000)
                
                if (i + 1) % 100 == 0:
                    print(f"  Progress: {i + 1}/{ITERATIONS} iterations completed")
        
        # Calculate statistics
        avg_iteration_time = sum(iteration_times) / len(iteration_times)
        min_iteration_time = min(iteration_times)
        max_iteration_time = max(iteration_times)
        
    except Exception as ex:
        print(f"Error: {ex}")
        print(f"Type: {type(ex).__name__}")
        import traceback
        traceback.print_exc()
        return
    
    # Stop performance measurement
    end_time = time.time()
    end_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    total_execution_time = (end_time - start_time) * 1000  # Convert to ms
    memory_used = end_memory - start_memory
    
    print("=== Performance Metrics ===")
    print(f"Total Iterations: {ITERATIONS}")
    print(f"Total Execution Time: {total_execution_time:.0f} ms")
    print(f"Average Time per Iteration: {avg_iteration_time:.3f} ms")
    print(f"Min Iteration Time: {min_iteration_time:.3f} ms")
    print(f"Max Iteration Time: {max_iteration_time:.3f} ms")
    print(f"Memory Used: {memory_used:.2f} MB")
    print("========================\n")
    
    # Export metrics to JSON file
    metrics_data = {
        "TestInfo": {
            "Language": "Python",
            "Framework": "Python",
            "Provider": "Ollama",
            "Model": model_name,
            "Endpoint": endpoint,
            "Timestamp": datetime.now(timezone.utc).isoformat(),
            "WarmupSuccessful": warmup_successful
        },
        "Metrics": {
            "TotalIterations": ITERATIONS,
            "TotalExecutionTimeMs": total_execution_time,
            "AverageTimePerIterationMs": avg_iteration_time,
            "MinIterationTimeMs": min_iteration_time,
            "MaxIterationTimeMs": max_iteration_time,
            "MemoryUsedMB": memory_used
        }
    }
    
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%d_%H%M%S")
    output_filename = f"metrics_python_ollama_{timestamp}.json"
    with open(output_filename, 'w') as f:
        json.dump(metrics_data, f, indent=2)
    print(f"✓ Metrics exported to: {output_filename}\n")


if __name__ == "__main__":
    asyncio.run(run_performance_test())
