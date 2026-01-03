"""Minimal local shim for agent_framework to support sample performance scripts.

This shim is not a full implementation of Microsoft Agent Framework. It provides
just enough surface area for the sample agents in this repository to execute
without requiring the external package.
"""

__all__ = ["azure", "ollama"]
