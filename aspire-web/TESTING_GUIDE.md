# Testing Guide for Aspire Performance Comparison Web Application

This guide helps you test the complete Aspire-based performance comparison web application.

## Prerequisites Checklist

Before testing, ensure you have:

- [ ] **.NET 10.0 SDK** installed
- [ ] **Python 3.10+** installed
- [ ] **Ollama** installed and running
- [ ] A model pulled in Ollama (e.g., `ollama pull ministral-3`)
- [ ] **Git** repository cloned

### Verify Prerequisites

```bash
# Check .NET version
dotnet --version
# Should show 10.0.x or later

# Check Python version
python --version
# Should show Python 3.10 or later

# Check Ollama is running
curl http://localhost:11434/api/tags
# Should return JSON with available models

# Verify model is available
ollama list
# Should show ministral-3 or your chosen model
```

## Test Scenarios

### Test 1: Build the Solution

**Purpose**: Verify all projects compile without errors

```bash
cd aspire-web
dotnet restore
dotnet build
```

**Expected Result**: 
- All projects restore successfully
- Build completes with 0 errors (warnings are OK)
- Output shows "Build succeeded"

---

### Test 2: Run Full Application with Aspire

**Purpose**: Test complete orchestrated solution

```bash
cd aspire-web/AppHost/PerformanceComparison.AppHost
dotnet run
```

**Expected Result**:
- Aspire dashboard opens in browser automatically
- Shows all services in the dashboard
- Console shows service URLs
- No errors in console

**Navigate to Web Application**:
1. Find the web URL in the console (usually https://localhost:XXXX)
2. Open in browser
3. Click "Performance Comparison" in navigation menu

---

### Test 3: Run Performance Test

**Purpose**: Verify end-to-end performance testing

**Steps**:
1. In the web UI, ensure:
   - Iterations: 10 (small test)
   - Model: ministral-3 (or your model)
   - Endpoint: http://localhost:11434
   - Test Mode: standard
2. Click "Refresh Status" button
3. Verify both backends show "Healthy"
4. Click "Run Performance Tests"

**Expected Result**:
- Button shows spinner and "Running Tests..."
- After ~5-30 seconds, results appear
- Both .NET and Python results display
- Comparison Summary shows winner

---

## Success Criteria

The application is working correctly if:

✅ All projects build without errors  
✅ Aspire dashboard shows all services  
✅ Web UI loads and is interactive  
✅ Can configure and run tests  
✅ Results display for both .NET and Python  
✅ Comparison shows performance winner  

---

For full testing details, see the complete testing guide in the aspire-web folder.
