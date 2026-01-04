import asyncio
import os
import time
import psutil
import uuid
from datetime import datetime, timezone
from typing import Optional, Dict
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

class TestSession:
    def __init__(self, session_id: str, config: TestConfiguration):
        self.session_id = session_id
        self.configuration = config
        self.status = "Running"
        self.start_time = datetime.now(timezone.utc)
        self.current_iteration = 0
        self.total_iterations = config.iterations
        self.elapsed_time_ms = 0
        self.iteration_times = []
        self.warmup_successful = False
        self.memory_used_mb = 0
        self.machine_info = {}
        self.error_message = None
        self.task = None
        self.cancel_event = asyncio.Event()

# Global state
current_session: Optional[TestSession] = None
sessions: Dict[str, TestSession] = {}

@app.get("/")
async def root():
    return {"message": "Python Performance Backend", "status": "running"}

@app.get("/api/performance/health")
async def health():
    return {"status": "healthy", "service": "python-backend"}

@app.post("/api/performance/start")
async def start_test(config: TestConfiguration):
    global current_session
    
    # Stop any existing test
    if current_session and current_session.status == "Running":
        current_session.cancel_event.set()
        current_session.status = "Stopped"
    
    session_id = str(uuid.uuid4())
    session = TestSession(session_id, config)
    sessions[session_id] = session
    current_session = session
    
    # Start background task
    session.task = asyncio.create_task(execute_test(session))
    
    return {"sessionId": session_id, "message": "Test started successfully"}

@app.post("/api/performance/stop")
async def stop_test():
    global current_session
    
    if current_session and current_session.status == "Running":
        current_session.cancel_event.set()
        current_session.status = "Stopped"
        return {"stopped": True, "message": "Test stopped successfully"}
    
    return {"stopped": False, "message": "No test running"}

@app.get("/api/performance/status")
async def get_status(sessionId: Optional[str] = None):
    global current_session
    
    session = None
    if sessionId:
        session = sessions.get(sessionId)
    elif current_session:
        session = current_session
    
    if not session:
        return {"status": "Idle", "message": "No test running"}
    
    avg_time = sum(session.iteration_times) / len(session.iteration_times) if session.iteration_times else 0
    min_time = min(session.iteration_times) if session.iteration_times else 0
    max_time = max(session.iteration_times) if session.iteration_times else 0
    
    progress_percentage = (session.current_iteration / session.total_iterations * 100) if session.total_iterations > 0 else 0
    
    return {
        "sessionId": session.session_id,
        "status": session.status,
        "currentIteration": session.current_iteration,
        "totalIterations": session.total_iterations,
        # Ensure elapsedTimeMs is an integer number of milliseconds so C# can deserialize to Int64
        "elapsedTimeMs": int(session.elapsed_time_ms),
        "progressPercentage": progress_percentage,
        "averageTimePerIterationMs": avg_time,
        "minIterationTimeMs": min_time,
        "maxIterationTimeMs": max_time,
        "memoryUsedMB": session.memory_used_mb,
        "warmupSuccessful": session.warmup_successful,
        "errorMessage": session.error_message,
        "configuration": session.configuration.dict(),
        "machineInfo": session.machine_info
    }

async def execute_test(session: TestSession):
    try:
        start_time = time.time()
        process = psutil.Process(os.getpid())
        start_memory = process.memory_info().rss / 1024 / 1024
        
        # Set environment variables
        os.environ["OLLAMA_HOST"] = session.configuration.endpoint
        os.environ["OLLAMA_CHAT_MODEL_ID"] = session.configuration.model
        
        # Create agent
        agent = OllamaChatClient(model_id=session.configuration.model).create_agent(
            name="PerformanceTestAgent",
            instructions="You are a helpful assistant. Provide brief, concise responses.",
        )
        
        # Warmup call
        try:
            warmup_start = time.time()
            await agent.run("Hello, this is a warmup call.")
            warmup_end = time.time()
            session.warmup_successful = True
        except Exception as ex:
            print(f"Warmup failed: {ex}")
        
        # Run iterations
        for i in range(session.configuration.iterations):
            if session.cancel_event.is_set():
                break
            
            iteration_start = time.time()
            try:
                await agent.run(f"Say hello {i + 1}")
            except Exception as ex:
                print(f"Iteration {i + 1} failed: {ex}")
            iteration_end = time.time()
            
            session.iteration_times.append((iteration_end - iteration_start) * 1000)
            session.current_iteration = i + 1
            session.elapsed_time_ms = (time.time() - start_time) * 1000
        
        end_time = time.time()
        end_memory = process.memory_info().rss / 1024 / 1024
        session.elapsed_time_ms = (end_time - start_time) * 1000
        session.memory_used_mb = end_memory - start_memory
        session.machine_info = get_machine_info()
        
        if session.cancel_event.is_set():
            session.status = "Stopped"
        else:
            session.status = "Completed"
        
    except Exception as ex:
        session.status = "Failed"
        session.error_message = str(ex)
        print(f"Test failed: {ex}")

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
