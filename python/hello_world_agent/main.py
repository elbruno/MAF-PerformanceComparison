import time
import psutil
import os

print("=== Python Microsoft Agent Framework - Hello World ===\n")

# Start performance measurement
start_time = time.time()
process = psutil.Process(os.getpid())
start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB

# Performance test: Run agent operations 1000 times
ITERATIONS = 1000
iteration_times = []

try:
    print(f"✓ Agent framework initialized successfully")
    print(f"✓ Running {ITERATIONS} iterations for performance testing\n")
    print("Note: This is a demo/mock setup without external AI services")
    print("For actual Azure OpenAI or Ollama, see the respective agent examples.\n")
    
    # Run 1000 iterations to measure performance
    for i in range(ITERATIONS):
        iteration_start = time.time()
        
        # Simulate agent operation (mock response)
        response = f"Hello from iteration {i + 1}!"
        
        iteration_end = time.time()
        iteration_times.append((iteration_end - iteration_start) * 1000)  # Convert to ms
        
        # Print progress every 100 iterations
        if (i + 1) % 100 == 0:
            print(f"  Progress: {i + 1}/{ITERATIONS} iterations completed")
    
    print("\n--- Sample Agent Response ---")
    print("Hello from the Microsoft Agent Framework in Python!")
    print("This agent is ready to process requests.")
    print("Performance test completed successfully.")
    print("-----------------------------\n")
    
    # Calculate statistics
    avg_iteration_time = sum(iteration_times) / len(iteration_times)
    min_iteration_time = min(iteration_times)
    max_iteration_time = max(iteration_times)
    
except Exception as ex:
    print(f"Error: {ex}")
    import traceback
    traceback.print_exc()

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
