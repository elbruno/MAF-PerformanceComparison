#!/usr/bin/env python3
"""
Performance test runner for Scenario 2: Enhanced Metrics (Production-Ready)

This script runs tests for both .NET and Python Agent Framework implementations
using industry-standard enhanced performance metrics.

Workflow:
1. Configure environment variables for tests
2. Clean up old metrics files
3. Run specified agent tests (.NET and Python with enhanced metrics)
4. Process results and generate comprehensive comparison reports
5. Generate AI-driven analysis using Ollama

Note: This is Scenario 2 - enhanced metrics for production use.
Uses PerformanceUtils (.NET) and performance_utils (Python) for accurate measurements.

Supported test modes: standard, batch, concurrent, streaming, scenarios
Supported agent types: HelloWorld, AzureOpenAI, Ollama, All
"""

import argparse
import glob
import json
import os
import platform
import shutil
import subprocess
import sys
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

from dotenv import load_dotenv

# Optional dependency with graceful fallback
try:
    import requests

    REQUESTS_AVAILABLE = True
except ImportError:
    REQUESTS_AVAILABLE = False


DEFAULT_TEST_MODE = "standard"
DEFAULT_MODEL = "ministral-3"
COMPARISON_TEMPLATE_PATH = os.path.join("..", "docs", "comparison_prompt_template.md")

# Color codes for cross-platform output
class Colors:
    """Cross-platform color output support."""

    CYAN = "\033[36m" if platform.system() != "Windows" else ""
    GREEN = "\033[32m" if platform.system() != "Windows" else ""
    YELLOW = "\033[33m" if platform.system() != "Windows" else ""
    RED = "\033[31m" if platform.system() != "Windows" else ""
    RESET = "\033[0m" if platform.system() != "Windows" else ""


def print_colored(message: str, color: str = "WHITE") -> None:
    """Print colored output, with graceful fallback for Windows."""
    color_map = {
        "CYAN": Colors.CYAN,
        "GREEN": Colors.GREEN,
        "YELLOW": Colors.YELLOW,
        "RED": Colors.RED,
    }
    color_code = color_map.get(color.upper(), "")
    reset = Colors.RESET if color_code else ""
    print(f"{color_code}{message}{reset}")


def find_python_executable() -> Optional[str]:
    """Find available Python executable."""
    for cmd in ["py", "python3", "python"]:
        try:
            result = subprocess.run([cmd, "--version"], capture_output=True, check=False)
            if result.returncode == 0:
                return cmd
        except FileNotFoundError:
            continue
    return None


def clean_old_metrics(script_dir: str) -> int:
    """Remove old metrics files before running tests."""
    print_colored("Cleaning up old metrics files...", "CYAN")

    metrics_patterns = [
        os.path.join(script_dir, "metrics_*.json"),
        os.path.join(script_dir, "dotnet", "**", "metrics_*.json"),
        os.path.join(script_dir, "python", "**", "metrics_*.json"),
    ]

    deleted_count = 0
    for pattern in metrics_patterns:
        for metrics_file in glob.glob(pattern, recursive=True):
            # Skip files in tests_results folder
            if "tests_results" not in metrics_file.replace("\\", "/"):
                try:
                    os.remove(metrics_file)
                    print(f"  Deleted: {os.path.basename(metrics_file)}")
                    deleted_count += 1
                except Exception as e:
                    print(f"  Failed to delete {metrics_file}: {e}")

    if deleted_count > 0:
        print_colored(f"Cleaned up {deleted_count} old metrics file(s)", "GREEN")
    else:
        print_colored("No old metrics files found", "GREEN")
    print()


def run_dotnet_test(
    agent_dir: str, agent_name: str, test_config: Dict[str, Any]
) -> bool:
    """Run a .NET agent test."""
    if not os.path.isdir(agent_dir):
        print_colored(
            f"Skipping .NET {agent_name}: directory not found: {agent_dir}",
            "YELLOW",
        )
        return True

    print_colored(f"Running .NET {agent_name} test...", "YELLOW")

    # Set environment variables
    env = os.environ.copy()
    env["TEST_MODE"] = test_config["test_mode"]
    env["ITERATIONS"] = str(test_config["iterations"])
    env["BATCH_SIZE"] = str(test_config["batch_size"])
    env["CONCURRENT_REQUESTS"] = str(test_config["concurrent_requests"])
    env["OLLAMA_MODEL_NAME"] = test_config["model"]

    try:
        # Build
        result = subprocess.run(
            ["dotnet", "build"],
            cwd=agent_dir,
            env=env,
            capture_output=True,
            check=False,
        )
        if result.returncode != 0:
            print_colored(f"Failed to build .NET project: {result.stderr.decode()}", "RED")
            return False

        # Run
        result = subprocess.run(
            ["dotnet", "run"],
            cwd=agent_dir,
            env=env,
            check=False,
        )

        if result.returncode == 0:
            print_colored(f"[OK] .NET {agent_name} test completed", "GREEN")
        else:
            print_colored(f"[FAILED] .NET {agent_name} test failed", "RED")
            return False

    except Exception as e:
        print_colored(f"Error running .NET test: {e}", "RED")
        return False

    print()
    return True


def run_python_test(
    agent_dir: str, agent_name: str, test_config: Dict[str, Any]
) -> bool:
    """Run a Python agent test."""
    if not os.path.isdir(agent_dir):
        print_colored(
            f"Skipping Python {agent_name}: directory not found: {agent_dir}",
            "YELLOW",
        )
        return True

    print_colored(f"Running Python {agent_name} test...", "YELLOW")

    # Set environment variables
    env = os.environ.copy()
    env["TEST_MODE"] = test_config["test_mode"]
    env["ITERATIONS"] = str(test_config["iterations"])
    env["BATCH_SIZE"] = str(test_config["batch_size"])
    env["CONCURRENT_REQUESTS"] = str(test_config["concurrent_requests"])
    env["OLLAMA_MODEL_NAME"] = test_config["model"]
    env["OLLAMA_CHAT_MODEL_ID"] = test_config["model"]

    python_exe = find_python_executable()
    if not python_exe:
        print_colored(
            f"Python not found on PATH. To run Python tests, install Python.",
            "YELLOW",
        )
        print_colored(f"Skipping Python {agent_name} test.", "YELLOW")
        print()
        return True

    try:
        result = subprocess.run(
            [python_exe, "main.py"],
            cwd=agent_dir,
            env=env,
            check=False,
        )

        if result.returncode == 0:
            print_colored(f"[OK] Python {agent_name} test completed", "GREEN")
        else:
            print_colored(f"[FAILED] Python {agent_name} test failed", "RED")
            req_file = os.path.join(agent_dir, "requirements.txt")
            if os.path.isfile(req_file):
                print_colored(
                    f"To install requirements run: {python_exe} -m pip install -r {req_file}",
                    "YELLOW",
                )
            return False

    except Exception as e:
        print_colored(f"Error running Python test: {e}", "RED")
        return False

    print()
    return True


def run_agent_tests(script_dir: str, agent_type: str, test_config: Dict[str, Any]) -> bool:
    """Execute tests for specified agent type."""
    success = True

    if agent_type in ["HelloWorld", "All"]:
        success &= run_dotnet_test(
            os.path.join(script_dir, "dotnet", "HelloWorldAgent"),
            "HelloWorld",
            test_config,
        )
        success &= run_python_test(
            os.path.join(script_dir, "python", "hello_world_agent"),
            "HelloWorld",
            test_config,
        )

    if agent_type in ["AzureOpenAI", "All"]:
        success &= run_dotnet_test(
            os.path.join(script_dir, "dotnet", "AzureOpenAIAgent"),
            "AzureOpenAI",
            test_config,
        )
        success &= run_python_test(
            os.path.join(script_dir, "python", "azure_openai_agent"),
            "AzureOpenAI",
            test_config,
        )

    if agent_type in ["Ollama", "All"]:
        success &= run_dotnet_test(
            os.path.join(script_dir, "dotnet", "OllamaAgent"),
            "Ollama",
            test_config,
        )
        success &= run_python_test(
            os.path.join(script_dir, "python", "ollama_agent"),
            "Ollama",
            test_config,
        )

    return success


# ============================================================================
# Results Processing Functions (from original process_results_ollama.py)
# ============================================================================


def find_metrics_files(search_dir: str = ".") -> List[str]:
    """Find all NEW metrics JSON files (excluding tests_results folder)."""

    patterns = [
        os.path.join(search_dir, "metrics_*.json"),
        os.path.join(search_dir, "dotnet", "**", "metrics_*.json"),
        os.path.join(search_dir, "python", "**", "metrics_*.json"),
    ]

    files: List[str] = []
    for pattern in patterns:
        matched_files = glob.glob(pattern, recursive=True)
        # Exclude files in tests_results folder
        files.extend([f for f in matched_files if "tests_results" not in f.replace("\\", "/")])

    return list(set(files))


def parse_metrics_filename(filename: str) -> Dict[str, str]:
    """Parse metrics filename to extract metadata."""

    basename = os.path.basename(filename)
    parts = basename.replace("metrics_", "").replace(".json", "").split("_")

    info: Dict[str, str] = {
        "language": parts[0] if len(parts) > 0 else "unknown",
        "provider": parts[1] if len(parts) > 1 else "unknown",
        "test_mode": DEFAULT_TEST_MODE,
        "timestamp": "",
    }

    if len(parts) >= 4:
        test_modes = ["standard", "batch", "concurrent", "streaming", "scenarios"]
        if parts[2] in test_modes:
            info["test_mode"] = parts[2]
            info["timestamp"] = "_".join(parts[3:])
        else:
            info["timestamp"] = "_".join(parts[2:])
    elif len(parts) >= 3:
        info["timestamp"] = "_".join(parts[2:])

    return info


def load_metrics_file(filepath: str) -> Dict[str, Any]:
    """Load and return the contents of a metrics JSON file, with metadata."""

    try:
        with open(filepath, "r", encoding="utf-8") as f:
            data = json.load(f)

        result = data.copy()
        result["_filename"] = os.path.basename(filepath)
        result["_filepath"] = filepath
        return result
    except Exception as exc:  # pragma: no cover - defensive logging
        print(f"Warning: Could not load {filepath}: {exc}")
        return {}


def load_comparison_template() -> str:
    """Load the comparison prompt template from docs; fall back to an inline default."""

    default_template = (
        "I have two performance test results from running the Microsoft Agent Framework in "
        "different environments. Please analyze and compare these results.\n\n"
        "**First Test Result:**\n```json\n{first}\n```\n\n"
        "**Second Test Result:**\n```json\n{second}\n```\n\n"
        "Please provide a comprehensive comparison that includes:\n\n"
        "1. **Test Configuration Comparison:**\n"
        "   - Language/Framework used (C#/.NET vs Python)\n"
        "   - AI Provider and Model\n"
        "   - Warmup status\n"
        "   - Test timestamp\n\n"
        "2. **Performance Metrics Analysis:**\n"
        "   - Total execution time comparison (which is faster and by what percentage?)\n"
        "   - Average time per iteration (which has better average performance?)\n"
        "   - Min/Max iteration times (which has more consistent performance?)\n"
        "   - Performance variability (compare min/max ranges)\n\n"
        "3. **Resource Usage:**\n"
        "   - Memory consumption comparison\n"
        "   - Efficiency analysis (performance vs resource usage)\n\n"
        "4. **Key Insights:**\n"
        "   - Which implementation performs better overall?\n"
        "   - What are the notable differences?\n"
        "   - Any recommendations based on the results?\n\n"
        "5. **Statistical Summary:**\n"
        "   - Provide a clear winner or note if results are comparable\n"
        "   - Highlight any significant performance gaps\n"
        "   - Consider both speed and resource efficiency\n\n"
        "Please format your response in a clear, structured way with percentages and concrete numbers for easy understanding."
    )

    try:
        with open(COMPARISON_TEMPLATE_PATH, "r", encoding="utf-8") as f:
            content = f.read()
        # Use the instruction section from the template while keeping placeholders
        marker = "Please provide a comprehensive comparison"
        instruction_idx = content.find(marker)

        instructions = content[instruction_idx:].strip() if instruction_idx != -1 else default_template

        return (
            "I have two performance test results from running the Microsoft Agent Framework in "
            "different environments. Please analyze and compare these results.\n\n"
            "**First Test Result:**\n```json\n{first}\n```\n\n"
            "**Second Test Result:**\n```json\n{second}\n```\n\n"
            + instructions
        )
    except Exception:
        return default_template


def filter_ollama_metrics(metrics_files: List[str]) -> List[Dict[str, Any]]:
    """Load metrics files and keep only those whose provider is Ollama."""

    ollama_metrics: List[Dict[str, Any]] = []
    for filepath in metrics_files:
        data = load_metrics_file(filepath)
        if not data:
            continue

        provider = (data.get("TestInfo", {}).get("Provider") or "").lower()
        if provider == "ollama":
            ollama_metrics.append(data)

    return ollama_metrics


def build_comparison_prompt(
    first_metrics: Dict[str, Any],
    second_metrics: Dict[str, Any],
    template_body: str,
) -> str:
    """Fill the comparison template with two metrics payloads."""

    first_json = json.dumps(first_metrics, indent=2)
    second_json = json.dumps(second_metrics, indent=2)

    if "{first}" in template_body and "{second}" in template_body:
        return template_body.replace("{first}", first_json).replace("{second}", second_json)

    # Fall back: compose prompt using the template instructions
    return (
        "I have two performance test results from running the Microsoft Agent Framework in "
        "different environments. Please analyze and compare these results.\n\n"
        f"**First Test Result:**\n```json\n{first_json}\n```\n\n"
        f"**Second Test Result:**\n```json\n{second_json}\n```\n\n"
        + template_body
    )


def group_metrics_by_mode(metrics: List[Dict[str, Any]]) -> Dict[str, Dict[str, Dict[str, Any]]]:
    """Group metrics by test mode and language for pairing."""

    grouped: Dict[str, Dict[str, Dict[str, Any]]] = {}
    for entry in metrics:
        test_info = entry.get("TestInfo", {})
        test_mode = test_info.get("TestMode", DEFAULT_TEST_MODE)
        language = test_info.get("Language", "unknown")

        key = test_mode
        if key not in grouped:
            grouped[key] = {}
        grouped[key][language] = entry

    return grouped


def create_comparison_markdown(
    metrics: List[Dict[str, Any]],
    output_dir: str,
    template_body: str,
    test_mode: str = "standard",
    iterations: int = 1000,
) -> Tuple[str, List[Tuple[str, str, str]]]:
    """
    Create comparison_report_{testmode}_{iterations}iter.md and return its path plus a list of prompts generated.

    The prompts list contains tuples of (title, prompt_text, test_mode).
    """

    markdown_lines: List[str] = [
        "# Performance Comparison Report",
        "",
        f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        "This report summarizes Ollama performance test results and provides prompts to generate detailed comparisons.",
        "",
        "## Overview",
        "",
        f"Found {len(metrics)} Ollama test run(s).",
        "",
        "## Test Results",
        "",
    ]

    prompts: List[Tuple[str, str, str]] = []

    grouped = group_metrics_by_mode(metrics)
    for test_mode, language_map in grouped.items():
        if len(language_map) < 2:
            continue

        languages = sorted(language_map.keys())
        first_lang, second_lang = languages[:2]
        first_metrics = language_map[first_lang]
        second_metrics = language_map[second_lang]

        prompt_text = build_comparison_prompt(first_metrics, second_metrics, template_body)
        prompts.append((f"Ollama - {test_mode}", prompt_text, test_mode))

        markdown_lines.extend(
            [
                f"### Comparison: Ollama - {test_mode}",
                "",
                "#### Files Analyzed",
                f"- {first_metrics['_filename']}",
                f"- {second_metrics['_filename']}",
                "",
                "#### LLM Comparison Prompt",
                "",
                "```",
                prompt_text,
                "```",
                "",
                "---",
                "",
            ]
        )

    markdown_lines.extend(["## Individual Test Summaries", ""])

    for entry in metrics:
        test_info = entry.get("TestInfo", {})
        metrics_data = entry.get("Metrics", {})
        machine_info = entry.get("MachineInfo", {})

        markdown_lines.extend(
            [
                f"### {test_info.get('Language', 'Unknown')} - Ollama - {test_info.get('TestMode', DEFAULT_TEST_MODE)}",
                "",
                f"**File:** `{entry['_filename']}`",
                "",
                "**Test Information:**",
                f"- Language/Framework: {test_info.get('Language', 'N/A')} / {test_info.get('Framework', 'N/A')}",
                f"- Provider: {test_info.get('Provider', 'N/A')}",
                f"- Model: {test_info.get('Model', 'N/A')}",
                f"- Test Mode: {test_info.get('TestMode', DEFAULT_TEST_MODE)}",
                f"- Endpoint: {test_info.get('Endpoint', 'N/A')}",
                f"- Timestamp: {test_info.get('Timestamp', 'N/A')}",
                f"- Warmup Successful: {test_info.get('WarmupSuccessful', False)}",
                "",
                "**Machine Information:**",
                f"- OS: {machine_info.get('OSSystem', 'N/A')} {machine_info.get('OSRelease', '')}",
                f"- Architecture: {machine_info.get('Architecture', 'N/A')}",
                f"- Processors: {machine_info.get('ProcessorCount', 'N/A')} cores ({machine_info.get('LogicalProcessorCount', 'N/A')} logical)",
            ]
        )

        # Add runtime information if available
        if machine_info.get("DotNetVersion"):
            markdown_lines.append(f"- .NET Version: {machine_info.get('DotNetVersion')}")

        # Add CPU frequency if available
        if machine_info.get("CPUMaxFreqGHz"):
            markdown_lines.append(f"- CPU Max Frequency: {machine_info.get('CPUMaxFreqGHz')} GHz")
        if machine_info.get("CPUMaxSpeedGHz"):
            markdown_lines.append(f"- CPU Max Speed: {machine_info.get('CPUMaxSpeedGHz')} GHz")

        # Add memory info
        if machine_info.get("TotalMemoryGB"):
            markdown_lines.append(f"- Total Memory: {machine_info.get('TotalMemoryGB')} GB")
        if machine_info.get("AvailableMemoryGB"):
            markdown_lines.append(f"- Available Memory: {machine_info.get('AvailableMemoryGB')} GB")

        # Add GPU info if available
        if machine_info.get("GPUModel"):
            markdown_lines.append(f"- GPU: {machine_info.get('GPUModel')} ({machine_info.get('GPUMemoryMB', 'N/A')})")

        # Add Python version if available
        if machine_info.get("PythonVersion"):
            markdown_lines.append(f"- Python Version: {machine_info.get('PythonVersion')}")

        markdown_lines.extend(
            [
                "",
                "**Performance Metrics:**",
                f"- Total Iterations: {metrics_data.get('TotalIterations', 'N/A')}",
                f"- Total Execution Time: {metrics_data.get('TotalExecutionTimeMs', 'N/A')} ms",
                f"- Average Time per Iteration: {metrics_data.get('AverageTimePerIterationMs', 'N/A')} ms",
                f"- Min Iteration Time: {metrics_data.get('MinIterationTimeMs', 'N/A')} ms",
                f"- Max Iteration Time: {metrics_data.get('MaxIterationTimeMs', 'N/A')} ms",
                f"- Median Iteration Time: {metrics_data.get('MedianIterationTimeMs', 'N/A')}",
                f"- Standard Deviation: {metrics_data.get('StandardDeviationMs', 'N/A')} ms",
                f"- Memory Used: {metrics_data.get('MemoryUsedMB', 'N/A')} MB",
                f"- Average CPU Usage: {metrics_data.get('AverageCpuUsagePercent', 'N/A')}%",
            ]
        )

        if metrics_data.get("TimeToFirstTokenMs") is not None:
            markdown_lines.append(
                f"- Time to First Token: {metrics_data.get('TimeToFirstTokenMs', 'N/A')} ms"
            )

        if entry.get("Summary"):
            markdown_lines.extend(["", f"**Summary:** {entry.get('Summary')}"])

        markdown_lines.extend(["", "---", ""])

    markdown_content = "\n".join(markdown_lines)
    output_file = os.path.join(output_dir, f"comparison_report_{test_mode}_{iterations}iter.md")

    with open(output_file, "w", encoding="utf-8") as f:
        f.write(markdown_content)

    return output_file, prompts


def analyze_with_ollama(prompt: str, endpoint: str, model: str) -> Optional[str]:
    """Analyze the comparison using an Ollama model."""

    if not REQUESTS_AVAILABLE:
        print("Error: requests module not available; skip Ollama analysis.")
        return None

    try:
        payload = {"model": model, "prompt": prompt, "stream": False}
        response = requests.post(f"{endpoint}/api/generate", json=payload, timeout=180)

        if response.status_code == 200:
            result = response.json()
            return result.get("response", "")

        print(f"Error: Ollama returned status {response.status_code}")
        return None
    except Exception as exc:  # pragma: no cover - defensive logging
        print(f"Error calling Ollama: {exc}")
        return None


def analyze_prompts(
    prompts: List[Tuple[str, str, str]],
    output_dir: str,
    endpoint: str,
    model: str,
    test_mode: str = "standard",
    iterations: int = 1000,
) -> Optional[str]:
    """Run Ollama analysis for each prompt and store in analysis_report_{testmode}_{iterations}iter.md."""

    if not prompts:
        return None

    analysis_lines: List[str] = [
        "# Performance Analysis Report",
        "",
        f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        f"Analysis Provider: OLLAMA ({model})",
        "",
        "---",
        "",
    ]

    for title, prompt_text, test_mode in prompts:
        analysis_lines.extend([f"## {title}", "", f"Test Mode: {test_mode}", ""])

        result = analyze_with_ollama(prompt_text, endpoint, model)
        if result:
            analysis_lines.append(result.strip())
        else:
            analysis_lines.append("Analysis could not be generated for this prompt.")

        analysis_lines.extend(["", "---", ""])

    analysis_content = "\n".join(analysis_lines)
    analysis_file = os.path.join(output_dir, f"analysis_report_{test_mode}_{iterations}iter.md")

    with open(analysis_file, "w", encoding="utf-8") as f:
        f.write(analysis_content)

    return analysis_file


def ensure_results_folder(test_mode: str, iterations: int = 1000) -> str:
    """Create the destination folder inside tests_results."""

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    folder_name = f"{timestamp}_{test_mode}_{iterations}iter"
    tests_results_dir = "tests_results"

    os.makedirs(tests_results_dir, exist_ok=True)

    new_folder = os.path.join(tests_results_dir, folder_name)
    os.makedirs(new_folder, exist_ok=True)
    return new_folder


def determine_test_mode(metrics_files: List[str]) -> str:
    """Infer test mode from the most recent metrics file content or name."""

    if not metrics_files:
        return DEFAULT_TEST_MODE

    most_recent_file = max(metrics_files, key=os.path.getmtime)
    info = parse_metrics_filename(most_recent_file)

    metrics_data = load_metrics_file(most_recent_file)
    test_info = metrics_data.get("TestInfo", {}) if metrics_data else {}
    return test_info.get("TestMode", info.get("test_mode", DEFAULT_TEST_MODE))


def determine_iterations(metrics: List[Dict[str, Any]]) -> int:
    """Extract iterations count from metrics, preferring the most common value."""

    if not metrics:
        return 1000

    # Get iterations from first metric that has it
    for entry in metrics:
        iterations = entry.get("Metrics", {}).get("TotalIterations")
        if iterations:
            return int(iterations)

    return 1000


def copy_metrics_files(metrics_files: List[str], destination: str) -> List[str]:
    """Move metrics files into the destination folder and return their new paths."""

    moved_files: List[str] = []
    for metrics_file in metrics_files:
        basename = os.path.basename(metrics_file)
        dest_path = os.path.join(destination, basename)
        try:
            shutil.move(metrics_file, dest_path)
            moved_files.append(dest_path)
            print(f"  ✓ Moved: {basename}")
        except Exception as exc:
            print(f"  ✗ Failed to move {basename}: {exc}")
    return moved_files


def pick_ollama_model(metrics: List[Dict[str, Any]]) -> str:
    """Select an Ollama model: env > first metrics TestInfo.Model > default."""

    model = os.getenv("OLLAMA_MODEL_NAME") or os.getenv("OLLAMA_CHAT_MODEL_ID")
    if model:
        return model

    for entry in metrics:
        model_name = entry.get("TestInfo", {}).get("Model")
        if model_name:
            return model_name

    return DEFAULT_MODEL


def process_results(script_dir: str) -> int:
    """Process test results and generate reports."""

    print_colored("=" * 60, "GREEN")
    print_colored("Ollama - Performance Results Processor", "GREEN")
    print_colored("=" * 60, "GREEN")
    print()
    print("Searching for metrics files...")

    all_metrics_files = find_metrics_files()
    if not all_metrics_files:
        print("No metrics files found. Please run tests first.")
        return 1

    print(f"Found {len(all_metrics_files)} metrics file(s). Filtering for Ollama...")
    ollama_metrics = filter_ollama_metrics(all_metrics_files)

    if not ollama_metrics:
        print("No Ollama metrics found. Ensure provider is Ollama in your tests.")
        return 1

    # Determine destination folder based on most recent file's test mode and iterations
    test_mode = determine_test_mode([m.get("_filepath", "") for m in ollama_metrics if m.get("_filepath")])
    iterations = determine_iterations(ollama_metrics)
    destination_folder = ensure_results_folder(test_mode, iterations)
    print(f"Created folder: {destination_folder}")
    print()

    print("Moving new metrics files...")
    copied_files = copy_metrics_files(
        [m["_filepath"] for m in ollama_metrics if m.get("_filepath")],
        destination_folder,
    )
    print()

    # Reload from current location to ensure paths/filenames are accurate
    reloaded_metrics = [load_metrics_file(path) for path in copied_files if path]

    # Determine test mode and iterations for report naming
    test_mode = determine_test_mode([path for path in copied_files if path])
    iterations = determine_iterations(reloaded_metrics)

    template_body = load_comparison_template()
    print("Generating comparison report...")
    comparison_file, prompts = create_comparison_markdown(
        reloaded_metrics, destination_folder, template_body, test_mode, iterations
    )
    print(f"  ✓ Created: {os.path.basename(comparison_file)}")
    print()

    endpoint = os.getenv("OLLAMA_ENDPOINT", "http://localhost:11434")
    model = pick_ollama_model(reloaded_metrics)
    print(f"Analyzing comparison prompts with Ollama model '{model}' at {endpoint}...")
    analysis_file = analyze_prompts(prompts, destination_folder, endpoint, model, test_mode, iterations)
    if analysis_file:
        print(f"  ✓ Created: {os.path.basename(analysis_file)}")
    else:
        print("  ✗ Analysis was skipped or failed.")
    print()

    print_colored("=" * 60, "GREEN")
    print_colored("Processing complete!", "GREEN")
    print_colored("=" * 60, "GREEN")
    print()
    print(f"Results location: {destination_folder}")
    print(f"Files moved: {len(copied_files)}")
    print(f"Comparison report: {comparison_file}")
    if analysis_file:
        print(f"Analysis report: {analysis_file}")
    print()
    print("Next steps:")
    if analysis_file:
        print("1. Review analysis_report.md for automated insights.")
        print("2. Review comparison_report.md for detailed metrics and prompts.")
    else:
        print("1. Review comparison_report.md and use the embedded prompts with your LLM.")

    return 0


# ============================================================================
# Main Entry Point
# ============================================================================


def main() -> int:
    """Main entry point for unified performance test runner."""

    parser = argparse.ArgumentParser(
        description="Run Microsoft Agent Framework performance tests and generate reports",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Run Ollama tests with 100 iterations
  python run_performance_tests.py -i 100 -a Ollama
  
  # Run all agent types in batch mode
  python run_performance_tests.py -m batch -a All
  
  # Run concurrent tests with 50 requests
  python run_performance_tests.py -m concurrent -c 50
  
  # Process results only (without running tests)
  python run_performance_tests.py --process-only
        """,
    )

    parser.add_argument(
        "-a",
        "--agent-type",
        default="Ollama",
        choices=["HelloWorld", "AzureOpenAI", "Ollama", "All"],
        help="Agent type to test (default: Ollama)",
    )
    parser.add_argument(
        "-m",
        "--test-mode",
        default="standard",
        choices=["standard", "batch", "concurrent", "streaming", "scenarios"],
        help="Test mode (default: standard)",
    )
    parser.add_argument(
        "-i",
        "--iterations",
        type=int,
        default=10,
        help="Number of test iterations (default: 10)",
    )
    parser.add_argument(
        "-b",
        "--batch-size",
        type=int,
        default=10,
        help="Batch size for batch mode (default: 10)",
    )
    parser.add_argument(
        "-c",
        "--concurrent-requests",
        type=int,
        default=5,
        help="Number of concurrent requests (default: 5)",
    )
    parser.add_argument(
        "--model",
        default=DEFAULT_MODEL,
        help="Ollama model to use for both .NET and Python agents (default: ministral-3)",
    )
    parser.add_argument(
        "--process-only",
        action="store_true",
        help="Process results only without running tests",
    )
    parser.add_argument(
        "--skip-cleanup",
        action="store_true",
        help="Skip cleaning old metrics files",
    )
    parser.add_argument(
        "--skip-analysis",
        action="store_true",
        help="Generate reports but skip Ollama analysis",
    )

    args = parser.parse_args()

    load_dotenv()

    script_dir = os.path.dirname(os.path.abspath(__file__))
    os.chdir(script_dir)

    # Display configuration
    print_colored("=" * 60, "CYAN")
    print_colored("Microsoft Agent Framework Performance Tests", "CYAN")
    print_colored("=" * 60, "CYAN")
    print()

    if args.process_only:
        print_colored("Running in process-only mode (results processing)", "CYAN")
        print()
        return process_results(script_dir)

    print_colored("Configuration:", "GREEN")
    print(f"  Agent Type: {args.agent_type}")
    print(f"  Test Mode: {args.test_mode}")
    print(f"  Iterations: {args.iterations}")
    print(f"  Model: {args.model}")
    if args.test_mode == "batch":
        print(f"  Batch Size: {args.batch_size}")
    if args.test_mode == "concurrent":
        print(f"  Concurrent Requests: {args.concurrent_requests}")
    print()

    # Clean up old metrics
    if not args.skip_cleanup:
        clean_old_metrics(script_dir)

    # Create test configuration
    test_config = {
        "test_mode": args.test_mode,
        "iterations": args.iterations,
        "batch_size": args.batch_size,
        "concurrent_requests": args.concurrent_requests,
        "model": args.model,
    }

    # Run tests
    print_colored("Running tests...", "CYAN")
    print()
    if not run_agent_tests(script_dir, args.agent_type, test_config):
        print_colored("Some tests failed", "YELLOW")
        print()

    print_colored("=" * 60, "GREEN")
    print_colored("All tests completed!", "GREEN")
    print_colored("=" * 60, "GREEN")
    print()

    # Process results
    if args.skip_analysis:
        print("Skipping results processing (--skip-analysis flag set)")
        return 0

    print("Processing results...")
    print()
    return process_results(script_dir)


if __name__ == "__main__":
    raise SystemExit(main())
