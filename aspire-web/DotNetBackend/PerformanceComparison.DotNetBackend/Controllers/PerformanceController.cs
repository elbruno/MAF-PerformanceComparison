using Microsoft.AspNetCore.Mvc;
using PerformanceComparison.DotNetBackend.Models;
using PerformanceComparison.DotNetBackend.Services;

namespace PerformanceComparison.DotNetBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly BackgroundTestService _backgroundTestService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        BackgroundTestService backgroundTestService,
        ILogger<PerformanceController> logger)
    {
        _backgroundTestService = backgroundTestService;
        _logger = logger;
    }

    [HttpPost("start")]
    public IActionResult StartTest([FromBody] TestConfiguration config)
    {
        _logger.LogInformation("Starting test with {Iterations} iterations", config.Iterations);
        
        var sessionId = _backgroundTestService.StartTest(config);
        
        return Ok(new { sessionId, message = "Test started successfully" });
    }

    [HttpPost("stop")]
    public IActionResult StopTest()
    {
        _logger.LogInformation("Stopping test");
        
        var stopped = _backgroundTestService.StopTest();
        
        return Ok(new { stopped, message = stopped ? "Test stopped successfully" : "No test running" });
    }

    [HttpGet("status")]
    public IActionResult GetStatus([FromQuery] string? sessionId = null)
    {
        var session = _backgroundTestService.GetStatus(sessionId);
        
        if (session == null)
        {
            return Ok(new { status = "Idle", message = "No test running" });
        }

        var avgTime = session.IterationTimes.Any() ? session.IterationTimes.Average() : 0;
        var minTime = session.IterationTimes.Any() ? session.IterationTimes.Min() : 0;
        var maxTime = session.IterationTimes.Any() ? session.IterationTimes.Max() : 0;

        return Ok(new
        {
            sessionId = session.SessionId,
            status = session.Status,
            currentIteration = session.CurrentIteration,
            totalIterations = session.TotalIterations,
            elapsedTimeMs = session.ElapsedTimeMs,
            progressPercentage = session.TotalIterations > 0 ? (session.CurrentIteration / (double)session.TotalIterations) * 100 : 0,
            averageTimePerIterationMs = avgTime,
            minIterationTimeMs = minTime,
            maxIterationTimeMs = maxTime,
            memoryUsedMB = session.MemoryUsedMB,
            warmupSuccessful = session.WarmupSuccessful,
            errorMessage = session.ErrorMessage,
            configuration = session.Configuration,
            machineInfo = session.MachineInfo
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "dotnet-backend" });
    }
}
