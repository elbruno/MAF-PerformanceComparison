# Changelog - Aspire Web Application

## [2.0.0] - 2026-01-04

### Major Refactoring - Background Test Execution with Real-time Updates

#### Backend Changes

**New Architecture:**
- Tests now run in separate background processes instead of blocking HTTP requests
- Implemented background task management with cancellation support
- Session-based tracking of test execution state

**New API Endpoints:**
- `POST /api/performance/start` - Starts a test in background, returns sessionId
- `POST /api/performance/stop` - Stops currently running test
- `GET /api/performance/status?sessionId={id}` - Gets real-time status of test
- `GET /api/performance/health` - Health check (unchanged)

**Status Information Includes:**
- Current iteration number / Total iterations
- Elapsed time in milliseconds
- Progress percentage (0-100%)
- Average/Min/Max iteration times
- Memory usage
- Warmup status
- Test status (Running, Completed, Stopped, Failed)
- Machine information
- Error messages (if any)

**Implementation Details:**

*.NET Backend (`BackgroundTestService.cs`):*
- Uses `CancellationTokenSource` for test cancellation
- Runs test execution in background `Task`
- Thread-safe session management with `ConcurrentDictionary`
- Single active test at a time with automatic cleanup

*Python Backend (`main.py`):*
- Uses `asyncio.create_task()` for background execution
- Event-based cancellation with `asyncio.Event()`
- Global session tracking
- Async/await throughout for non-blocking operations

#### Frontend Changes

**New Features:**
1. **Auto-Polling**: Fetches status every 2 seconds during test execution
2. **Collapsible Cards**: All sections can be collapsed/expanded by clicking header
3. **Export Results**: Download test results as JSON file
4. **Stop Tests**: Cancel running tests at any time
5. **Real-time Progress**: Progress bars show completion percentage
6. **Cleaned UI**: Removed unnecessary pages (Counter, Weather, Home)

**UI Components:**
- Test Configuration card (collapsible)
- Service Status card (collapsible)
- .NET Results card (collapsible, with progress bar)
- Python Results card (collapsible, with progress bar)
- Comparison Summary card (collapsible, shown when both tests complete)

**User Flow:**
1. Configure test parameters (iterations, model, endpoint)
2. Click "Start Tests" - both backends start simultaneously
3. Frontend polls status every 2 seconds
4. Real-time updates show progress, iterations, elapsed time
5. Progress bars visualize completion percentage
6. Can stop tests at any time with "Stop Tests" button
7. When completed, comparison summary appears
8. Export results to JSON file with "Export Results" button

**Removed Pages:**
- `/` (Home) - now redirects to `/dashboard`
- `/counter` (Counter demo)
- `/weather` (Weather demo)

**Navigation:**
- Simplified to single "Performance Dashboard" link
- Automatically redirects root to dashboard

#### Technical Improvements

**JavaScript Integration:**
- Added `downloadFile()` function for JSON export
- Integrated Bootstrap Icons for better visuals

**State Management:**
- Component-level state for collapse status
- Timer-based polling with automatic cleanup
- Implements `IDisposable` for proper resource cleanup

**Error Handling:**
- Graceful handling of backend failures
- Clear error messages displayed to user
- Status polling continues even if one request fails

**Performance:**
- Non-blocking test execution
- Efficient polling (2-second intervals)
- Minimal data transfer (only status updates)

#### Breaking Changes

- Removed `POST /api/performance/run` endpoint (replaced with start/stop/status)
- Changed response models for new API endpoints
- Frontend no longer waits for test completion
- Test results not returned synchronously

#### Migration Guide

**For Users:**
- Navigate to `/dashboard` (automatic redirect from `/`)
- Use "Start Tests" instead of "Run Performance Tests"
- Results update in real-time (no need to wait for completion)
- Use "Stop Tests" to cancel anytime
- Use "Export Results" to save results

**For Developers:**
- Update API calls to use new endpoints
- Implement polling if integrating externally
- Use sessionId for tracking specific test runs

#### Benefits

1. **Better UX**: Real-time feedback instead of waiting for completion
2. **More Control**: Ability to stop long-running tests
3. **Cleaner UI**: Collapsible sections reduce clutter
4. **Easier Export**: One-click JSON download
5. **Focused App**: Removed demo pages for production-ready feel
6. **Scalable**: Background execution doesn't block server

---

## [1.0.0] - 2026-01-04

### Initial Release

- Aspire-based orchestration of .NET and Python backends
- Blazor Server frontend
- Synchronous test execution
- Basic results display
- Multiple pages (Home, Counter, Weather, Performance)
