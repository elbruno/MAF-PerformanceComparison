# Aspire Web Scenario - Quick Guide

This document summarizes the `aspire-web/` scenario: a real-time web dashboard that runs and compares Microsoft Agent Framework performance tests using Aspire for orchestration.

Quick goals

- Provide an interactive web UI to configure and run performance tests for .NET and Python agent implementations
- Run tests in the background and stream progress to the UI via polling
- Collect and export performance metrics (JSON) for later analysis
- Orchestrate the frontend and both backends with .NET Aspire AppHost

Quick start (recommended)

```bash
cd aspire-web/AppHost/PerformanceComparison.AppHost
dotnet run
```

Then open the dashboard URL shown in the console and navigate to `/dashboard`.

Run components individually

- .NET backend: `cd aspire-web/DotNetBackend/PerformanceComparison.DotNetBackend && dotnet run`
- Python backend: `cd aspire-web/PythonBackend && pip install -r requirements.txt && python main.py`
- Web frontend: `cd aspire-web/Web/PerformanceComparison.Web && dotnet run`

Where metrics are saved

- The backends write `metrics_dotnet_*.json` and `metrics_python_*.json` files at test completion (these files are ignored by git unless placed under `tests_results/`).

Where to find more docs

- Full guide: `docs/ASPIRE_WEB.md` (moved here from root)
- Implementation notes: `docs/IMPLEMENTATION_SUMMARY.md`

If you want me to also move additional aspire-web docs into this folder or convert them to a single consolidated guide, say the word and I’ll proceed.
