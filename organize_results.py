#!/usr/bin/env python3
"""
Organize test results and create comparison markdown files.

This script:
1. Creates a new timestamped folder in tests_results/
2. Moves all metrics_*.json files to the new folder
3. Generates a comparison markdown file ready for LLM analysis
"""

import os
import json
import shutil
import glob
from datetime import datetime
from typing import List, Dict, Any


# Constants
DEFAULT_TEST_MODE = "standard"


def find_metrics_files(search_dir: str = ".") -> List[str]:
    """Find all metrics JSON files in the specified directory and subdirectories."""
    patterns = [
        os.path.join(search_dir, "metrics_*.json"),
        os.path.join(search_dir, "dotnet", "**", "metrics_*.json"),
        os.path.join(search_dir, "python", "**", "metrics_*.json"),
    ]
    
    files = []
    for pattern in patterns:
        files.extend(glob.glob(pattern, recursive=True))
    
    return list(set(files))  # Remove duplicates


def parse_metrics_filename(filename: str) -> Dict[str, str]:
    """Parse metrics filename to extract metadata."""
    basename = os.path.basename(filename)
    # Format: metrics_{language}_{provider}_{testmode}_{timestamp}.json
    parts = basename.replace("metrics_", "").replace(".json", "").split("_")
    
    info = {
        "language": parts[0] if len(parts) > 0 else "unknown",
        "provider": parts[1] if len(parts) > 1 else "unknown",
        "test_mode": DEFAULT_TEST_MODE,
        "timestamp": ""
    }
    
    # Try to extract test mode and timestamp
    if len(parts) >= 4:
        # Check if third part is a test mode
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
    """Load and return the contents of a metrics JSON file."""
    try:
        with open(filepath, 'r') as f:
            data = json.load(f)
            # Create a copy and add metadata fields
            result = data.copy()
            result["_filename"] = os.path.basename(filepath)
            result["_filepath"] = filepath
            return result
    except Exception as e:
        print(f"Warning: Could not load {filepath}: {e}")
        return {}


def create_comparison_markdown(metrics_files: List[str], output_dir: str) -> str:
    """Create a comparison markdown file based on the template."""
    
    # Load all metrics
    all_metrics = []
    for filepath in metrics_files:
        data = load_metrics_file(filepath)
        if data:
            all_metrics.append(data)
    
    if len(all_metrics) < 2:
        print("Warning: Need at least 2 metrics files for comparison")
        return ""
    
    # Group by test mode and provider
    grouped = {}
    for metrics in all_metrics:
        test_info = metrics.get("TestInfo", {})
        test_mode = test_info.get("TestMode", DEFAULT_TEST_MODE)
        provider = test_info.get("Provider", "unknown")
        language = test_info.get("Language", "unknown")
        
        key = f"{provider}_{test_mode}"
        if key not in grouped:
            grouped[key] = {}
        grouped[key][language] = metrics
    
    # Generate markdown content
    markdown_lines = [
        "# Performance Comparison Report",
        "",
        f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        "## Overview",
        "",
        f"This report contains performance comparison data from {len(all_metrics)} test runs.",
        "",
        "## Test Results",
        ""
    ]
    
    # Add comparison prompts for each group
    for group_key, group_data in grouped.items():
        # Only create comparison if we have at least 2 different languages
        if len(group_data) < 2:
            continue
            
        # Get the two implementations to compare
        languages = sorted(list(group_data.keys()))  # Sort for consistency
        first_lang = languages[0]
        second_lang = languages[1]
        
        first_metrics = group_data[first_lang]
        second_metrics = group_data[second_lang]
        
        provider = first_metrics.get("TestInfo", {}).get("Provider", "unknown")
        test_mode = first_metrics.get("TestInfo", {}).get("TestMode", DEFAULT_TEST_MODE)
        
        markdown_lines.extend([
            f"### Comparison: {provider} - {test_mode} mode",
            "",
            "#### Files Analyzed",
            f"- {first_metrics['_filename']}",
            f"- {second_metrics['_filename']}",
            "",
            "#### LLM Comparison Prompt",
            "",
            "```",
            "I have two performance test results from running the Microsoft Agent Framework in different environments. Please analyze and compare these results.",
            "",
            "**First Test Result:**",
            "```json",
            json.dumps(first_metrics, indent=2),
            "```",
            "",
            "**Second Test Result:**",
            "```json",
            json.dumps(second_metrics, indent=2),
            "```",
            "",
            "Please provide a comprehensive comparison that includes:",
            "",
            "1. **Test Configuration Comparison:**",
            "   - Language/Framework used (C#/.NET vs Python)",
            "   - AI Provider and Model",
            "   - Test mode and configuration",
            "   - Warmup status",
            "   - Test timestamp",
            "",
            "2. **Performance Metrics Analysis:**",
            "   - Total execution time comparison (which is faster and by what percentage?)",
            "   - Average time per iteration (which has better average performance?)",
            "   - Min/Max iteration times (which has more consistent performance?)",
            "   - Performance variability (compare min/max ranges)",
            "   - Standard deviation analysis",
            "",
            "3. **Resource Usage:**",
            "   - Memory consumption comparison",
            "   - CPU utilization comparison",
            "   - Efficiency analysis (performance vs resource usage)",
            "",
            "4. **Test Mode Specific Metrics:**",
            "   - Batch processing efficiency (if applicable)",
            "   - Concurrent request handling (if applicable)",
            "   - Streaming performance and TTFT (if applicable)",
            "   - Scenario-specific results (if applicable)",
            "",
            "5. **Key Insights:**",
            "   - Which implementation performs better overall?",
            "   - What are the notable differences?",
            "   - Any recommendations based on the results?",
            "",
            "6. **Statistical Summary:**",
            "   - Provide a clear winner or note if results are comparable",
            "   - Highlight any significant performance gaps",
            "   - Consider both speed and resource efficiency",
            "",
            "Please format your response in a clear, structured way with percentages and concrete numbers for easy understanding.",
            "```",
            "",
            "---",
            ""
        ])
    
    # Add individual test summaries
    markdown_lines.extend([
        "## Individual Test Summaries",
        ""
    ])
    
    for metrics in all_metrics:
        test_info = metrics.get("TestInfo", {})
        metrics_data = metrics.get("Metrics", {})
        
        markdown_lines.extend([
            f"### {test_info.get('Language', 'Unknown')} - {test_info.get('Provider', 'Unknown')} - {test_info.get('TestMode', DEFAULT_TEST_MODE)}",
            "",
            f"**File:** `{metrics['_filename']}`",
            "",
            f"**Test Information:**",
            f"- Language/Framework: {test_info.get('Language', 'N/A')} / {test_info.get('Framework', 'N/A')}",
            f"- Provider: {test_info.get('Provider', 'N/A')}",
            f"- Model: {test_info.get('Model', 'N/A')}",
            f"- Test Mode: {test_info.get('TestMode', 'standard')}",
            f"- Timestamp: {test_info.get('Timestamp', 'N/A')}",
            f"- Warmup Successful: {test_info.get('WarmupSuccessful', False)}",
            "",
            f"**Performance Metrics:**",
            f"- Total Iterations: {metrics_data.get('TotalIterations', 'N/A')}",
            f"- Total Execution Time: {metrics_data.get('TotalExecutionTimeMs', 'N/A')} ms",
            f"- Average Time per Iteration: {metrics_data.get('AverageTimePerIterationMs', 'N/A')} ms",
            f"- Min Iteration Time: {metrics_data.get('MinIterationTimeMs', 'N/A')} ms",
            f"- Max Iteration Time: {metrics_data.get('MaxIterationTimeMs', 'N/A')} ms",
            f"- Median Iteration Time: {metrics_data.get('MedianIterationTimeMs', 'N/A')} ms",
            f"- Standard Deviation: {metrics_data.get('StandardDeviationMs', 'N/A')} ms",
            f"- Memory Used: {metrics_data.get('MemoryUsedMB', 'N/A')} MB",
            f"- Average CPU Usage: {metrics_data.get('AverageCpuUsagePercent', 'N/A')}%",
        ])
        
        if metrics_data.get('TimeToFirstTokenMs') is not None:
            markdown_lines.append(f"- Time to First Token: {metrics_data.get('TimeToFirstTokenMs', 'N/A')} ms")
        
        if metrics.get('Summary'):
            markdown_lines.extend([
                "",
                f"**Summary:** {metrics.get('Summary')}",
            ])
        
        markdown_lines.extend(["", "---", ""])
    
    # Write markdown file
    markdown_content = "\n".join(markdown_lines)
    output_file = os.path.join(output_dir, "comparison_report.md")
    
    with open(output_file, 'w') as f:
        f.write(markdown_content)
    
    return output_file


def main():
    """Main function to organize test results."""
    print("=" * 60)
    print("Microsoft Agent Framework - Test Results Organizer")
    print("=" * 60)
    print()
    
    # Find all metrics files
    print("Searching for metrics files...")
    metrics_files = find_metrics_files()
    
    if not metrics_files:
        print("No metrics files found. Please run tests first.")
        return 1
    
    print(f"Found {len(metrics_files)} metrics file(s):")
    for f in metrics_files:
        print(f"  - {f}")
    print()
    
    # Determine test mode and timestamp for folder name
    # Use the most recent file's info
    most_recent_file = max(metrics_files, key=os.path.getmtime)
    file_info = parse_metrics_filename(most_recent_file)
    
    # Try to load the file to get better info
    metrics_data = load_metrics_file(most_recent_file)
    test_mode = "standard"
    if metrics_data:
        test_info = metrics_data.get("TestInfo", {})
        test_mode = test_info.get("TestMode", "standard")
    
    # Create timestamped folder
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    folder_name = f"{timestamp}_{test_mode}"
    
    # Create tests_results directory if it doesn't exist
    tests_results_dir = "tests_results"
    if not os.path.exists(tests_results_dir):
        os.makedirs(tests_results_dir)
        print(f"Created {tests_results_dir} directory")
    
    # Create the new folder
    new_folder = os.path.join(tests_results_dir, folder_name)
    os.makedirs(new_folder, exist_ok=True)
    print(f"Created folder: {new_folder}")
    print()
    
    # Move metrics files
    print("Moving metrics files...")
    moved_files = []
    for metrics_file in metrics_files:
        basename = os.path.basename(metrics_file)
        dest_path = os.path.join(new_folder, basename)
        
        try:
            shutil.move(metrics_file, dest_path)
            moved_files.append(dest_path)
            print(f"  ✓ Moved: {basename}")
        except Exception as e:
            print(f"  ✗ Failed to move {basename}: {e}")
    print()
    
    # Create comparison markdown
    if moved_files:
        print("Generating comparison report...")
        markdown_file = create_comparison_markdown(moved_files, new_folder)
        if markdown_file:
            print(f"  ✓ Created: {os.path.basename(markdown_file)}")
        print()
    
    # Summary
    print("=" * 60)
    print("Results organized successfully!")
    print("=" * 60)
    print()
    print(f"Results location: {new_folder}")
    print(f"Files moved: {len(moved_files)}")
    if markdown_file:
        print(f"Comparison report: {markdown_file}")
    print()
    print("Next steps:")
    print("1. Review the comparison_report.md file")
    print("2. Copy the LLM prompts to your preferred AI assistant")
    print("3. Analyze the performance differences")
    
    return 0


if __name__ == "__main__":
    exit(main())
