using Microsoft.AspNetCore.Mvc;
using PerformanceComparison.DotNetBackend.Models;
using PerformanceComparison.DotNetBackend.Services;

namespace PerformanceComparison.DotNetBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly PerformanceTestService _testService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        PerformanceTestService testService,
        ILogger<PerformanceController> logger)
    {
        _testService = testService;
        _logger = logger;
    }

    [HttpPost("run")]
    public async Task<ActionResult<TestResult>> RunTest([FromBody] TestConfiguration config)
    {
        _logger.LogInformation("Received test request for {Iterations} iterations", config.Iterations);
        
        var result = await _testService.RunPerformanceTestAsync(config);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "dotnet-backend" });
    }
}
