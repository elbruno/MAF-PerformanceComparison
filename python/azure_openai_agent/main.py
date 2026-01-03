import asyncio
import json
import os
import time
import psutil
from random import randint
from typing import Annotated
from datetime import datetime

from agent_framework.azure import AzureAIClient
from azure.identity.aio import AzureCliCredential
from pydantic import Field
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

"""
Azure AI Agent Performance Test

This sample demonstrates basic usage of AzureAIClient with performance testing.
Shows both streaming and non-streaming responses with 1000 iteration testing.
"""


def get_weather(
    location: Annotated[str, Field(description="The location to get the weather for.")],
) -> str:
    """Get the weather for a given location."""
    conditions = ["sunny", "cloudy", "rainy", "stormy"]
    return f"The weather in {location} is {conditions[randint(0, 3)]} with a high of {randint(10, 30)}°C."


async def run_performance_test() -> None:
    """Run 1000 iterations of agent operations for performance testing."""
    print("=== Python Microsoft Agent Framework - Azure OpenAI Agent ===\n")
    
    # Configuration - Read from environment variables
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-5-mini")
    
    # Start performance measurement
    start_time = time.time()
    process = psutil.Process(os.getpid())
    start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    
    # Performance test: Run agent operations 1000 times
    ITERATIONS = 1000
    iteration_times = []
    warmup_successful = False
    
    try:
        if not endpoint:
            print("⚠ AZURE_OPENAI_ENDPOINT not found. Using demo mode.")
            print("To use Azure OpenAI, set the following environment variables:")
            print("  - AZURE_OPENAI_ENDPOINT")
            print("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, defaults to 'gpt-5-mini')")
            print(f"\n✓ Agent framework structure ready (demo mode)")
            print(f"✓ Running {ITERATIONS} iterations for performance testing\n")
            
            # Run 1000 iterations in demo mode (mock responses)
            for i in range(ITERATIONS):
                iteration_start = time.time()
                
                # Simulate agent operation (mock response)
                response = f"Hello from iteration {i + 1}! (Demo mode)"
                
                iteration_end = time.time()
                iteration_times.append((iteration_end - iteration_start) * 1000)
                
                if (i + 1) % 100 == 0:
                    print(f"  Progress: {i + 1}/{ITERATIONS} iterations completed")
        else:
            # For authentication, run `az login` command in terminal
            async with (
                AzureCliCredential() as credential,
                AzureAIClient(credential=credential).create_agent(
                    name="PerformanceTestAgent",
                    instructions="You are a helpful assistant. Provide brief, concise responses.",
                    tools=get_weather,
                ) as agent,
            ):
                print("✓ Agent framework initialized successfully")
                print("✓ Azure AI service configured")
                print(f"✓ Using deployment: {deployment_name}")
                
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
                
                # Run 1000 iterations with actual API calls
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
                async for chunk in agent.run_stream("What's the weather like in Seattle?"):
                    if chunk.text:
                        print(chunk.text, end="", flush=True)
                print("\n---------------------------\n")
        
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
            "Provider": "AzureOpenAI",
            "Model": deployment_name,
            "Endpoint": endpoint or "N/A (Demo Mode)",
            "Timestamp": datetime.utcnow().isoformat() + "Z",
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
    
    timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
    output_filename = f"metrics_python_azureopenai_{timestamp}.json"
    with open(output_filename, 'w') as f:
        json.dump(metrics_data, f, indent=2)
    print(f"✓ Metrics exported to: {output_filename}\n")


if __name__ == "__main__":
    asyncio.run(run_performance_test())
