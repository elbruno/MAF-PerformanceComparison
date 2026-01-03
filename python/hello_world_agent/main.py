import asyncio
import json
import time
import psutil
import os
import statistics
from datetime import datetime, timezone
from typing import List, Dict

print("=== Python Microsoft Agent Framework - Hello World ===\n")

# Configuration - Test modes
test_mode = os.getenv("TEST_MODE", "standard")  # standard, batch, concurrent, streaming, scenarios
ITERATIONS = int(os.getenv("ITERATIONS", "1000"))
BATCH_SIZE = int(os.getenv("BATCH_SIZE", "10"))
CONCURRENT_REQUESTS = int(os.getenv("CONCURRENT_REQUESTS", "5"))

# Comprehensive benchmarking scenarios
benchmark_scenarios = {
    "simple": "Say hello",
    "medium": "Explain what an AI agent is in one sentence",
    "long_output": "Write a detailed paragraph about the benefits of cloud computing",
    "reasoning": "If you have 3 apples and buy 2 more, then give away 1, how many do you have? Explain your reasoning",
    "conceptual": "What is the difference between machine learning and deep learning?"
}

# Start performance measurement
start_time = time.time()
process = psutil.Process(os.getpid())
start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB

iteration_times = []
cpu_samples = []
time_to_first_tokens = []
scenario_results = {}


async def run_standard_test(iterations: int, times: List[float], cpu_samples: List[float]) -> None:
    """Run standard sequential test"""
    for i in range(iterations):
        iteration_start = time.time()
        
        # Simulate agent operation (mock response)
        response = f"Hello from iteration {i + 1}!"
        
        iteration_end = time.time()
        times.append((iteration_end - iteration_start) * 1000)
        
        # Sample CPU every 100 iterations
        if (i + 1) % 100 == 0:
            cpu_samples.append(process.cpu_percent(interval=0.1))
            print(f"  Progress: {i + 1}/{iterations} iterations completed")


async def run_batch_test(iterations: int, batch_size: int, times: List[float], cpu_samples: List[float]) -> None:
    """Run batch processing test"""
    batches = (iterations + batch_size - 1) // batch_size
    for batch in range(batches):
        batch_start = time.time()
        current_batch_size = min(batch_size, iterations - batch * batch_size)
        
        # Process batch
        batch_tasks = []
        for i in range(current_batch_size):
            batch_tasks.append(asyncio.create_task(
                asyncio.sleep(0.001)  # Simulate work
            ))
        await asyncio.gather(*batch_tasks)
        
        batch_end = time.time()
        batch_time_ms = (batch_end - batch_start) * 1000
        
        # Record individual times (average for batch)
        avg_batch_time = batch_time_ms / current_batch_size
        for i in range(current_batch_size):
            times.append(avg_batch_time)
        
        cpu_samples.append(process.cpu_percent(interval=0.1))
        print(f"  Batch {batch + 1}/{batches} completed ({current_batch_size} items in {batch_time_ms:.3f} ms)")


async def run_concurrent_test(iterations: int, concurrent_req: int, times: List[float], cpu_samples: List[float]) -> None:
    """Run concurrent request test"""
    groups = (iterations + concurrent_req - 1) // concurrent_req
    for group in range(groups):
        group_start = time.time()
        current_group_size = min(concurrent_req, iterations - group * concurrent_req)
        
        async def process_request(request_id: int) -> float:
            start = time.time()
            await asyncio.sleep(0.001)  # Simulate work
            response = f"Concurrent response {request_id + 1}"
            end = time.time()
            return (end - start) * 1000
        
        tasks = [process_request(group * concurrent_req + i) for i in range(current_group_size)]
        results = await asyncio.gather(*tasks)
        times.extend(results)
        
        group_end = time.time()
        group_time_ms = (group_end - group_start) * 1000
        
        cpu_samples.append(process.cpu_percent(interval=0.1))
        print(f"  Group {group + 1}/{groups} completed ({current_group_size} concurrent requests in {group_time_ms:.3f} ms)")


async def run_streaming_test(iterations: int, times: List[float], ttfts: List[float], cpu_samples: List[float]) -> None:
    """Run streaming response test with time-to-first-token"""
    for i in range(iterations):
        iteration_start = time.time()
        
        # Simulate streaming response with time-to-first-token
        await asyncio.sleep(0.001)  # Simulate network delay
        first_token_time = time.time()
        ttft_ms = (first_token_time - iteration_start) * 1000
        ttfts.append(ttft_ms)
        
        # Continue simulating stream
        response = f"Streaming response {i + 1}"
        
        iteration_end = time.time()
        times.append((iteration_end - iteration_start) * 1000)
        
        if (i + 1) % 100 == 0:
            cpu_samples.append(process.cpu_percent(interval=0.1))
            print(f"  Progress: {i + 1}/{iterations} iterations completed")


async def run_scenarios_test(scenarios: Dict[str, str], times: List[float], 
                            scenario_results: Dict[str, List[float]], cpu_samples: List[float]) -> None:
    """Run comprehensive scenarios test"""
    for scenario_name, prompt in scenarios.items():
        print(f"Running scenario: {scenario_name}")
        scenario_times = []
        
        # Run each scenario 200 times
        for i in range(200):
            iteration_start = time.time()
            
            # Simulate processing the prompt
            response = f"Response to: {prompt}"
            if scenario_name == "long_output":
                await asyncio.sleep(0.005)  # Simulate longer processing
            
            iteration_end = time.time()
            iteration_time_ms = (iteration_end - iteration_start) * 1000
            scenario_times.append(iteration_time_ms)
            times.append(iteration_time_ms)
        
        scenario_results[scenario_name] = scenario_times
        cpu_samples.append(process.cpu_percent(interval=0.1))
        print(f"  Completed {scenario_name}: avg {statistics.mean(scenario_times):.3f} ms\n")


async def export_metrics(test_mode: str, total_time_ms: float, iteration_times: List[float],
                        memory_used: float, avg_cpu: float, ttfts: List[float],
                        scenarios: Dict[str, List[float]], batch_size: int, concurrent_requests: int) -> None:
    """Export comprehensive metrics to JSON"""
    current_timestamp = datetime.now(timezone.utc)
    
    metrics_data = {
        "TestInfo": {
            "Language": "Python",
            "Framework": "Python",
            "Provider": "HelloWorld",
            "Model": "N/A (Demo Mode)",
            "Endpoint": "N/A (Demo Mode)",
            "TestMode": test_mode,
            "Timestamp": current_timestamp.isoformat(),
            "WarmupSuccessful": False
        },
        "Configuration": {
            "BatchSize": batch_size,
            "ConcurrentRequests": concurrent_requests
        },
        "Metrics": {
            "TotalIterations": len(iteration_times),
            "TotalExecutionTimeMs": total_time_ms,
            "AverageTimePerIterationMs": statistics.mean(iteration_times),
            "MinIterationTimeMs": min(iteration_times),
            "MaxIterationTimeMs": max(iteration_times),
            "MedianIterationTimeMs": statistics.median(iteration_times),
            "StandardDeviationMs": statistics.stdev(iteration_times) if len(iteration_times) > 1 else 0,
            "MemoryUsedMB": memory_used,
            "AverageCpuUsagePercent": avg_cpu,
            "TimeToFirstTokenMs": statistics.mean(ttfts) if ttfts else None,
            "ScenarioResults": {
                name: {
                    "AverageMs": statistics.mean(times),
                    "MinMs": min(times),
                    "MaxMs": max(times),
                    "MedianMs": statistics.median(times)
                }
                for name, times in scenarios.items()
            } if scenarios else None
        },
        "Summary": generate_summary(test_mode, iteration_times, memory_used, avg_cpu, ttfts, scenarios)
    }
    
    timestamp = current_timestamp.strftime("%Y%m%d_%H%M%S")
    output_filename = f"metrics_python_helloworld_{test_mode}_{timestamp}.json"
    with open(output_filename, 'w') as f:
        json.dump(metrics_data, f, indent=2)
    print(f"✓ Metrics exported to: {output_filename}\n")


def generate_summary(test_mode: str, times: List[float], memory: float, cpu: float,
                    ttfts: List[float], scenarios: Dict[str, List[float]]) -> str:
    """Generate human-readable summary"""
    summary = f"Test completed in {test_mode} mode. "
    summary += f"Processed {len(times)} iterations with average latency of {statistics.mean(times):.3f}ms. "
    summary += f"Memory usage: {memory:.2f}MB, CPU usage: {cpu:.2f}%. "
    
    if ttfts:
        summary += f"Average time to first token: {statistics.mean(ttfts):.3f}ms. "
    
    if scenarios:
        fastest = min(scenarios.items(), key=lambda x: statistics.mean(x[1]))
        slowest = max(scenarios.items(), key=lambda x: statistics.mean(x[1]))
        summary += f"Fastest scenario: {fastest[0]} ({statistics.mean(fastest[1]):.3f}ms), "
        summary += f"Slowest scenario: {slowest[0]} ({statistics.mean(slowest[1]):.3f}ms). "
    
    return summary


async def main():
    """Main test execution"""
    try:
        print(f"✓ Agent framework initialized successfully")
        print(f"✓ Test mode: {test_mode}")
        print(f"✓ Running {ITERATIONS} iterations for performance testing\n")
        print("Note: This is a demo/mock setup without external AI services")
        print("For actual Azure OpenAI or Ollama, see the respective agent examples.\n")
        
        if test_mode.lower() == "batch":
            print(f"Running in BATCH mode with batch size: {BATCH_SIZE}\n")
            await run_batch_test(ITERATIONS, BATCH_SIZE, iteration_times, cpu_samples)
        elif test_mode.lower() == "concurrent":
            print(f"Running in CONCURRENT mode with {CONCURRENT_REQUESTS} concurrent requests\n")
            await run_concurrent_test(ITERATIONS, CONCURRENT_REQUESTS, iteration_times, cpu_samples)
        elif test_mode.lower() == "streaming":
            print("Running in STREAMING mode with time-to-first-token measurement\n")
            await run_streaming_test(ITERATIONS, iteration_times, time_to_first_tokens, cpu_samples)
        elif test_mode.lower() == "scenarios":
            print("Running COMPREHENSIVE SCENARIOS test\n")
            await run_scenarios_test(benchmark_scenarios, iteration_times, scenario_results, cpu_samples)
        else:
            print("Running in STANDARD mode\n")
            await run_standard_test(ITERATIONS, iteration_times, cpu_samples)
        
        print("\n--- Sample Agent Response ---")
        print("Hello from the Microsoft Agent Framework in Python!")
        print("This agent is ready to process requests.")
        print("Performance test completed successfully.")
        print("-----------------------------\n")
        
        # Calculate statistics
        avg_iteration_time = statistics.mean(iteration_times)
        min_iteration_time = min(iteration_times)
        max_iteration_time = max(iteration_times)
        avg_cpu = statistics.mean(cpu_samples) if cpu_samples else 0
        
    except Exception as ex:
        print(f"Error: {ex}")
        import traceback
        traceback.print_exc()
        return
    
    # Stop performance measurement
    end_time = time.time()
    end_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    total_execution_time = (end_time - start_time) * 1000  # Convert to ms
    memory_used = end_memory - start_memory
    
    print("=== Performance Metrics ===")
    print(f"Total Iterations: {len(iteration_times)}")
    print(f"Total Execution Time: {total_execution_time:.0f} ms")
    print(f"Average Time per Iteration: {avg_iteration_time:.3f} ms")
    print(f"Min Iteration Time: {min_iteration_time:.3f} ms")
    print(f"Max Iteration Time: {max_iteration_time:.3f} ms")
    print(f"Memory Used: {memory_used:.2f} MB")
    print(f"Average CPU Usage: {avg_cpu:.2f}%")
    
    if time_to_first_tokens:
        print(f"Average Time to First Token: {statistics.mean(time_to_first_tokens):.3f} ms")
    
    print("========================\n")
    
    # Export comprehensive metrics to JSON
    await export_metrics(test_mode, total_execution_time, iteration_times, memory_used,
                        avg_cpu, time_to_first_tokens, scenario_results, BATCH_SIZE, CONCURRENT_REQUESTS)


if __name__ == "__main__":
    asyncio.run(main())
