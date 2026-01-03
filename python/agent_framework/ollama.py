import asyncio
from dataclasses import dataclass
from typing import Any, Callable, Optional


@dataclass
class _Agent:
    name: str
    instructions: str
    tools: Optional[Callable[..., str]] = None

    async def run(self, prompt: str) -> str:
        # Simulated lightweight response
        return f"{self.name}: {prompt}"

    async def run_stream(self, prompt: str):
        # Simulated streaming chunks
        chunks = ["Simulated ", "streaming ", "response"]
        for text in chunks:
            await asyncio.sleep(0.001)
            yield type("Chunk", (), {"text": text})()


class OllamaChatClient:
    """Lightweight shim that mimics the minimal API used in samples."""

    def __init__(self, model_id: str, endpoint: Optional[str] = None) -> None:
        self.model_id = model_id
        self.endpoint = endpoint

    def create_agent(self, name: str, instructions: str, tools: Optional[Callable[..., Any]] = None) -> _Agent:
        return _Agent(name=name, instructions=instructions, tools=tools)
