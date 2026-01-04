import asyncio
import os
import time
import psutil
from datetime import datetime, timezone
from typing import Optional
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

from agent_framework.ollama import OllamaChatClient

app = FastAPI(title="Python Performance Backend")

# Add CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class TestConfiguration(BaseModel):
    iterations: int = 10
    model: str = "ministral-3"
    endpoint: str = "http://localhost:11434"
    test_mode: str = "standard"
    batch_size: int = 10
    concurrent_requests: int = 5

class TestMetrics(BaseModel):
    language: str = "Python"
    framework: str = "Python"
    provider: str = "Ollama"
    model: str
    endpoint: str
    timestamp: datetime
    warmup_successful: bool
    total_iterations: int
    total_execution_time_ms: float
    average_time_per_iteration_ms: float
    min_iteration_time_ms: float
    max_iteration_time_ms: float
    memory_used_mb: float
    machine_info: dict

class TestResult(BaseModel):
    success: bool
    message: str
    metrics: Optional[TestMetrics] = None

@app.get("/")
async def root():
    return {"message": "Python Performance Backend", "status": "running"}

@app.get("/api/performance/health")
async def health():
    return {"status": "healthy", "service": "python-backend"}

@app.post("/api/performance/run")
async def run_test(config: TestConfiguration) -> TestResult:
    try:
        start_time = time.time()
        process = psutil.Process(os.getpid())
        start_memory = process.memory_info().rss / 1024 / 1024
        
        iteration_times = []
        warmup_successful = False
        
        # Set environment variables for OllamaChatClient
        os.environ["OLLAMA_HOST"] = config.endpoint
        os.environ["OLLAMA_CHAT_MODEL_ID"] = config.model
        
        # Create agent
        agent = OllamaChatClient(model_id=config.model).create_agent(
            name="PerformanceTestAgent",
            instructions="You are a helpful assistant. Provide brief, concise responses.",
        )
        
        # Warmup call
        try:
            warmup_start = time.time()
            await agent.run("Hello, this is a warmup call.")
            warmup_end = time.time()
            warmup_time_ms = (warmup_end - warmup_start) * 1000
            warmup_successful = True
        except Exception as ex:
            print(f"Warmup failed: {ex}")
        
        # Run iterations
        for i in range(config.iterations):
            iteration_start = time.time()
            try:
                await agent.run(f"Say hello {i + 1}")
            except Exception as ex:
                print(f"Iteration {i + 1} failed: {ex}")
            iteration_end = time.time()
            iteration_times.append((iteration_end - iteration_start) * 1000)
        
        end_time = time.time()
        end_memory = process.memory_info().rss / 1024 / 1024
        total_execution_time = (end_time - start_time) * 1000
        memory_used = end_memory - start_memory
        
        metrics = TestMetrics(
            model=config.model,
            endpoint=config.endpoint,
            timestamp=datetime.now(timezone.utc),
            warmup_successful=warmup_successful,
            total_iterations=len(iteration_times),
            total_execution_time_ms=total_execution_time,
            average_time_per_iteration_ms=sum(iteration_times) / len(iteration_times),
            min_iteration_time_ms=min(iteration_times),
            max_iteration_time_ms=max(iteration_times),
            memory_used_mb=memory_used,
            machine_info=get_machine_info()
        )
        
        return TestResult(
            success=True,
            message="Test completed successfully",
            metrics=metrics
        )
        
    except Exception as ex:
        return TestResult(
            success=False,
            message=f"Test failed: {str(ex)}"
        )

def get_machine_info() -> dict:
    import platform
    machine_info = {
        "OSSystem": platform.system(),
        "OSRelease": platform.release(),
        "Architecture": platform.machine(),
        "ProcessorCount": psutil.cpu_count(logical=False),
        "LogicalProcessorCount": psutil.cpu_count(logical=True),
        "PythonVersion": platform.python_version(),
    }
    
    try:
        vm = psutil.virtual_memory()
        machine_info["TotalMemoryGB"] = round(vm.total / (1024**3), 2)
        machine_info["AvailableMemoryGB"] = round(vm.available / (1024**3), 2)
    except:
        pass
    
    return machine_info

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5001)
