import os
import time
import asyncio
import psutil
from dotenv import load_dotenv
from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.open_ai import OpenAIChatCompletion
from semantic_kernel.contents import ChatHistory

# Load environment variables
load_dotenv()

async def main():
    print("=== Python Microsoft Agent Framework - Ollama Agent ===\n")
    
    # Configuration - Read from environment variables or use defaults
    endpoint = os.getenv("OLLAMA_ENDPOINT", "http://localhost:11434")
    model_id = os.getenv("OLLAMA_MODEL_ID", "llama2")
    
    print(f"Configuring for Ollama endpoint: {endpoint}")
    print(f"Using model: {model_id}")
    print("\nNote: This requires Ollama to be running locally.")
    print("Install Ollama from: https://ollama.ai/")
    print(f"Start Ollama and pull the model: ollama pull {model_id}\n")
    
    # Start performance measurement
    start_time = time.time()
    process = psutil.Process(os.getpid())
    start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    
    try:
        # Create a kernel with OpenAI-compatible chat completion service for Ollama
        kernel = Kernel()
        
        # Ollama provides an OpenAI-compatible API
        # For Semantic Kernel, we use the OpenAI connector with custom URL
        from openai import AsyncOpenAI
        
        # Create a custom OpenAI client for Ollama
        client = AsyncOpenAI(
            api_key="not-used",  # Ollama doesn't require an API key
            base_url=f"{endpoint}/v1"
        )
        
        chat_service = OpenAIChatCompletion(
            ai_model_id=model_id,
            async_client=client
        )
        kernel.add_service(chat_service)
        
        print("✓ Kernel initialized successfully")
        print("✓ Ollama service configured")
        
        try:
            # Simple chat interaction
            chat_history = ChatHistory()
            chat_history.add_user_message("Hello! Please respond with a brief greeting in 10 words or less.")
            
            print("\n--- Attempting to connect to Ollama ---")
            response = await chat_service.get_chat_message_content(
                chat_history=chat_history,
                settings={}
            )
            
            print("\n--- Agent Response ---")
            print(response.content)
            print("----------------------\n")
        except Exception as connect_ex:
            print(f"\n⚠ Could not connect to Ollama at {endpoint}")
            print("Please ensure Ollama is running and the model is available.")
            print(f"Error: {connect_ex}")
            
    except Exception as ex:
        print(f"Error: {ex}")
        print(f"Type: {type(ex).__name__}")
    
    # Stop performance measurement
    end_time = time.time()
    end_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    execution_time = (end_time - start_time) * 1000  # Convert to ms
    memory_used = end_memory - start_memory
    
    print("=== Performance Metrics ===")
    print(f"Execution Time: {execution_time:.0f} ms")
    print(f"Memory Used: {memory_used:.2f} MB")
    print("========================\n")

if __name__ == "__main__":
    asyncio.run(main())
