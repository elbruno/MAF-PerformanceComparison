import asyncio
from contextlib import asynccontextmanager
from dataclasses import dataclass
from typing import Any, Callable, Optional


@dataclass
class _Agent:
    name: str
    instructions: str
    tools: Optional[Callable[..., str]] = None

    async def run(self, prompt: str) -> str:
        return f"{self.name}: {prompt}"

    async def run_stream(self, prompt: str):
        chunks = ["Simulated ", "streaming ", "response"]
        for text in chunks:
            await asyncio.sleep(0.001)
            yield type("Chunk", (), {"text": text})()

    async def __aenter__(self):
        return self

    async def __aexit__(self, exc_type, exc, tb):
        return False


class AzureAIClient:
    """Lightweight shim that mimics the minimal API used in samples."""

    def __init__(self, credential: Any, endpoint: Optional[str] = None) -> None:
        self.credential = credential
        self.endpoint = endpoint

    @asynccontextmanager
    async def create_agent(self, name: str, instructions: str, tools: Optional[Callable[..., Any]] = None):
        agent = _Agent(name=name, instructions=instructions, tools=tools)
        try:
            yield agent
        finally:
            pass
