import time
import psutil
import os
from semantic_kernel import Kernel

print("=== Python Microsoft Agent Framework - Hello World ===\n")

# Start performance measurement
start_time = time.time()
process = psutil.Process(os.getpid())
start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB

try:
    # Create a kernel
    # Note: This is a mock/demo setup. For actual Azure OpenAI or Ollama,
    # you would need to configure with actual endpoints and keys
    kernel = Kernel()
    
    # For demo purposes, we're showing the structure
    # In production, you would use:
    # from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
    # kernel.add_service(AzureChatCompletion(deployment_name, endpoint, api_key))
    # or for Ollama:
    # kernel.add_service(OpenAIChatCompletion(ai_model_id="llama2", api_key="", endpoint="http://localhost:11434"))
    
    print("✓ Kernel initialized successfully")
    print("✓ Agent framework ready")
    print("\n--- Hello World Agent Response ---")
    print("Hello from the Microsoft Agent Framework in Python!")
    print("This agent is ready to process requests.")
    print("-----------------------------------\n")
except Exception as ex:
    print(f"Error: {ex}")

# Stop performance measurement
end_time = time.time()
end_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
execution_time = (end_time - start_time) * 1000  # Convert to ms
memory_used = end_memory - start_memory

print("=== Performance Metrics ===")
print(f"Execution Time: {execution_time:.0f} ms")
print(f"Memory Used: {memory_used:.2f} MB")
print("========================\n")
