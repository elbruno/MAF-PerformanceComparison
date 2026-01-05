import asyncio
import json
import os
import time
import psutil
import platform
import sys
from datetime import datetime, timezone

# Add parent directory to path for performance_utils import
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from agent_framework.ollama import OllamaChatClient
from dotenv import load_dotenv
from performance_utils import PerformanceMetrics

# Load environment variables
load_dotenv()

"""
Ollama Agent Performance Test

This sample demonstrates implementing an Ollama agent with performance testing,
based on the reference implementation from microsoft/agent-framework.

Ensure to install Ollama and have a model running locally before running the sample.
Set the model to use via the OLLAMA_CHAT_MODEL_ID environment variable (default: ministral-3).

Reference: https://github.com/microsoft/agent-framework/blob/main/python/samples/getting_started/agents/ollama/ollama_agent_basic.py
https://ollama.com/
"""


def get_machine_info() -> dict:
    """Gather comprehensive machine information."""
    machine_info = {
        "OSSystem": platform.system(),
        "OSRelease": platform.release(),
        "OSVersion": platform.version(),
        "Architecture": platform.machine(),
        "ProcessorCount": psutil.cpu_count(logical=False),
        "LogicalProcessorCount": psutil.cpu_count(logical=True),
    }
    
    # CPU frequency info
    try:
        cpu_freq = psutil.cpu_freq()
        if cpu_freq:
            machine_info["CPUMaxFreqGHz"] = round(cpu_freq.max / 1000, 2)
            machine_info["CPUCurrentFreqGHz"] = round(cpu_freq.current / 1000, 2)
    except:
        pass
    
    # Memory info
    try:
        vm = psutil.virtual_memory()
        machine_info["TotalMemoryGB"] = round(vm.total / (1024**3), 2)
        machine_info["AvailableMemoryGB"] = round(vm.available / (1024**3), 2)
        machine_info["MemoryPercentUsed"] = vm.percent
    except:
        pass
    
    # Swap info
    try:
        swap = psutil.swap_memory()
        machine_info["TotalSwapGB"] = round(swap.total / (1024**3), 2)
    except:
        pass
    
    # GPU info (basic detection)
    try:
        # Try to detect NVIDIA GPU
        import subprocess
        result = subprocess.run(['nvidia-smi', '--query-gpu=name,memory.total', '--format=csv,noheader'],
                               capture_output=True, text=True, timeout=5)
        if result.returncode == 0 and result.stdout:
            gpu_info = result.stdout.strip().split(',')
            if len(gpu_info) >= 2:
                machine_info["GPUModel"] = gpu_info[0].strip()
                machine_info["GPUMemoryMB"] = gpu_info[1].strip()
    except:
        pass
    
    # Python version
    machine_info["PythonVersion"] = platform.python_version()
    
    return machine_info


def get_time(location: str) -> str:
    """Get the current time."""
    return f"The current time in {location} is {datetime.now().strftime('%I:%M %p')}."

async def run_performance_test() -> None:
    """Run 1000 iterations of agent operations for performance testing."""
    print("=== Python Microsoft Agent Framework - Ollama Agent ===\n")
    
    # Configuration - Read from environment variables or use defaults
    # Note: OllamaChatClient uses OLLAMA_HOST and OLLAMA_CHAT_MODEL_ID environment variables
    endpoint = os.getenv("OLLAMA_HOST", "http://localhost:11434")
    model_name = os.getenv("OLLAMA_CHAT_MODEL_ID", "ministral-3")
    
    print(f"Configuring for Ollama endpoint: {endpoint}")
    print(f"Using model: {model_name}")
    print("\nNote: This requires Ollama to be running locally.")
    print("Install Ollama from: https://ollama.com/")
    print(f"Start Ollama and pull the model: ollama pull {model_name}\n")
    
    # Performance test: Run agent operations. Make configurable via environment variable for easier testing.
    ITERATIONS = int(os.getenv("ITERATIONS", "1000"))
    warmup_successful = False
    
    # Create enhanced performance metrics tracker
    performance_metrics = PerformanceMetrics()
    performance_metrics.start()
    
    try:
        # Create agent using agent-framework with Ollama
        # Note: The model is configured via OLLAMA_CHAT_MODEL_ID environment variable
        agent = OllamaChatClient(model_id=model_name).create_agent(
            name="PerformanceTestAgent",
            instructions="You are a helpful assistant. Provide brief, concise responses.",
            tools=get_time,
        )
        print("✓ Agent framework initialized successfully")
        print("✓ Ollama service configured")

        # Warmup call - prepares the model for subsequent calls
        print("⏳ Performing warmup call to prepare the model...")
        warmup_start = time.time()
        await agent.run("Hello, this is a warmup call.")
        warmup_end = time.time()
        warmup_time_ms = (warmup_end - warmup_start) * 1000
        print(f"✓ Warmup completed in {warmup_time_ms:.3f} ms")
        warmup_successful = True
        
        print(f"✓ Running {ITERATIONS} iterations for performance testing\n")
        
        # Run iterations
        for i in range(ITERATIONS):
            iteration_start = time.time()
            await agent.run(f"Say hello {i + 1}")
            iteration_end = time.time()
            iteration_time_ms = (iteration_end - iteration_start) * 1000
            performance_metrics.record_measurement(iteration_time_ms)
            
            # Capture detailed snapshots periodically
            if (i + 1) % 100 == 0:
                performance_metrics.capture_memory_snapshot()
                performance_metrics.capture_cpu_snapshot()
                print(f"  Progress: {i + 1}/{ITERATIONS} iterations completed")
        
        print("\n--- Sample Agent Streaming Response ---")
        print("Agent: ", end="", flush=True)
        async for chunk in agent.run_stream("What time is it in Seattle?"):
            if getattr(chunk, "text", None):
                print(chunk.text, end="", flush=True)
        print("\n---------------------------\n")
        
    except Exception as ex:
        print(f"Error: {ex}")
        print(f"Type: {type(ex).__name__}")
        import traceback
        traceback.print_exc()
        raise
    
    # Get comprehensive metrics results
    result = performance_metrics.get_result()
    
    print("=== Enhanced Performance Metrics ===")
    print(f"Total Iterations: {ITERATIONS}")
    print(f"Total Execution Time: {result.total_elapsed_ms:.0f} ms")
    print("\nTiming Statistics:")
    print(f"  Mean: {result.mean:.3f} ms")
    print(f"  Median: {result.median:.3f} ms")
    print(f"  Min: {result.min:.3f} ms")
    print(f"  Max: {result.max:.3f} ms")
    print(f"  P90: {result.p90:.3f} ms")
    print(f"  P95: {result.p95:.3f} ms")
    print(f"  P99: {result.p99:.3f} ms")
    print(f"  StdDev: {result.stdev:.3f} ms")
    print("\nMemory Metrics:")
    print(f"  RSS Delta: {result.rss_delta_mb:.2f} MB")
    print(f"  VMS Delta: {result.vms_delta_mb:.2f} MB")
    print(f"  Peak RSS: {result.peak_rss_mb:.2f} MB")
    print("\nGarbage Collection:")
    print(f"  Gen0/Gen1/Gen2: {result.gc_gen0_collections}/{result.gc_gen1_collections}/{result.gc_gen2_collections}")
    print("\nCPU Metrics:")
    print(f"  Average CPU: {result.average_cpu_percent:.2f}%")
    print(f"  Max CPU: {result.max_cpu_percent:.2f}%")
    print("====================================\n")
    
    # Export enhanced metrics to JSON file
    current_timestamp = datetime.now(timezone.utc)
    machine_info = get_machine_info()
    metrics_data = {
        "TestInfo": {
            "Language": "Python",
            "Framework": "Python",
            "Provider": "Ollama",
            "Model": model_name,
            "Endpoint": endpoint,
            "Timestamp": current_timestamp.isoformat(),
            "WarmupSuccessful": warmup_successful
        },
        "MachineInfo": machine_info,
        "Metrics": {
            "TotalIterations": ITERATIONS,
            "TotalExecutionTimeMs": result.total_elapsed_ms,
            
            "Statistics": {
                "Mean": result.mean,
                "Median": result.median,
                "Min": result.min,
                "Max": result.max,
                "P90": result.p90,
                "P95": result.p95,
                "P99": result.p99,
                "StandardDeviation": result.stdev
            },
            
            "Memory": {
                "RSSDeltaMB": result.rss_delta_mb,
                "VMSDeltaMB": result.vms_delta_mb,
                "PeakRSSMB": result.peak_rss_mb,
                "PeakVMSMB": result.peak_vms_mb
            },
            
            "GarbageCollection": {
                "Gen0Collections": result.gc_gen0_collections,
                "Gen1Collections": result.gc_gen1_collections,
                "Gen2Collections": result.gc_gen2_collections
            },
            
            "CPU": {
                "AveragePercent": result.average_cpu_percent,
                "MaxPercent": result.max_cpu_percent
            },
            
            # Legacy fields for backward compatibility
            "AverageTimePerIterationMs": result.mean,
            "MinIterationTimeMs": result.min,
            "MaxIterationTimeMs": result.max,
            "MemoryUsedMB": result.rss_delta_mb
        }
    }
    
    timestamp = current_timestamp.strftime("%Y%m%d_%H%M%S")
    output_filename = f"metrics_python_ollama_{timestamp}.json"
    with open(output_filename, 'w') as f:
        json.dump(metrics_data, f, indent=2)
    print(f"✓ Metrics exported to: {output_filename}\n")


if __name__ == "__main__":
    asyncio.run(run_performance_test())
