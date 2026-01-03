# Ollama Agent Migration to agent-framework Reference

This document describes the changes made to align the Ollama agent implementation with the Microsoft Agent Framework reference sample.

## Reference Sample

The implementation now follows the official reference:

- <https://github.com/microsoft/agent-framework/blob/main/python/samples/getting_started/agents/ollama/ollama_agent_basic.py>

## Key Changes

### 1. Package Dependencies

**Before:**

```python
# Local shim implementation in python/agent_framework/ollama.py
```

**After:**

```python
# Official package from PyPI
agent-framework-ollama
```

### 2. OllamaChatClient Initialization

**Before:**

```python
agent = OllamaChatClient(model_id=model_name).create_agent(...)
```

**After:**

```python
agent = OllamaChatClient().create_agent(...)
```

The `OllamaChatClient()` constructor no longer requires the `model_id` parameter. The model is configured via environment variables.

### 3. Environment Variables

**Before:**

- `OLLAMA_ENDPOINT` - Ollama server endpoint
- `OLLAMA_MODEL_NAME` - Model to use

**After:**

- `OLLAMA_HOST` - Ollama server endpoint (used by agent-framework)
- `OLLAMA_MODEL_ID` - Model to use (used by agent-framework)

These environment variable names match the official agent-framework conventions.

### 4. Removed Local Shim

The local `python/agent_framework` folder (which contained shim implementations) has been completely removed. All agents now use official agent-framework packages:

- Ollama: `agent-framework-ollama`
- Azure AI: `agent-framework-azure`

## Installation

```bash
cd python/ollama_agent
pip install -r requirements.txt
```

## Configuration

Copy `.env.example` to `.env` and configure:

```bash
# OLLAMA_HOST is used by OllamaChatClient (defaults to http://localhost:11434)
OLLAMA_HOST=http://localhost:11434

# OLLAMA_MODEL_ID is used by OllamaChatClient to specify the model
# For function calling support, try: llama3.2, qwen2.5:8b, mistral
OLLAMA_MODEL_ID=llama3.2

# Optional: Number of iterations for performance testing (default: 1000)
# ITERATIONS=1000
```

## Usage

```bash
python main.py
```

## Recommended Models

As per the agent-framework documentation:

- For function calling: `llama3.2`, `qwen2.5:8b`, `mistral`
- For reasoning: `qwen3:8b`
- For multimodal: `gemma3:4b`

## Testing

The implementation now properly:

1. Initializes `OllamaChatClient` without parameters
2. Uses environment variables for configuration
3. Creates agents with proper tool support
4. Supports both streaming and non-streaming responses
5. Follows the exact pattern from the reference sample
