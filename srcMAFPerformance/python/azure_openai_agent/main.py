import os
import time
import asyncio
import psutil
from dotenv import load_dotenv
from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.contents import ChatHistory

# Load environment variables
load_dotenv()

# Default chat settings
DEFAULT_CHAT_SETTINGS = {}

async def main():
    print("=== Python Microsoft Agent Framework - Azure OpenAI Agent ===\n")
    
    # Configuration - Read from environment variables
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    api_key = os.getenv("AZURE_OPENAI_API_KEY")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4")
    
    # Start performance measurement
    start_time = time.time()
    process = psutil.Process(os.getpid())
    start_memory = process.memory_info().rss / 1024 / 1024  # Convert to MB
    
    try:
        if not endpoint or not api_key:
            print("⚠ Configuration not found. Using demo mode.")
            print("To use Azure OpenAI, set the following environment variables:")
            print("  - AZURE_OPENAI_ENDPOINT")
            print("  - AZURE_OPENAI_API_KEY")
            print("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, defaults to 'gpt-4')")
            print("\n✓ Kernel structure ready (demo mode)")
            print("✓ Agent framework initialized")
        else:
            # Create a kernel with Azure OpenAI chat completion service
            kernel = Kernel()
            
            # Add Azure OpenAI chat completion service
            chat_service = AzureChatCompletion(
                deployment_name=deployment_name,
                endpoint=endpoint,
                api_key=api_key
            )
            kernel.add_service(chat_service)
            
            print("✓ Kernel initialized successfully")
            print("✓ Azure OpenAI service connected")
            print(f"✓ Using deployment: {deployment_name}")
            
            # Simple chat interaction
            chat_history = ChatHistory()
            chat_history.add_user_message("Hello! Please respond with a brief greeting.")
            
            response = await chat_service.get_chat_message_content(
                chat_history=chat_history,
                settings=DEFAULT_CHAT_SETTINGS
            )
            
            print("\n--- Agent Response ---")
            print(response.content)
            print("----------------------\n")
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
