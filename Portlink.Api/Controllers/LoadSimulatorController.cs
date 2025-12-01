using PortlinkApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortlinkApp.Api.Controllers;

[ApiController]
[Route("api/load-simulator")]
[Authorize]
public class LoadSimulatorController : ControllerBase
{
    private readonly LoadSimulatorService _simulator;

    public LoadSimulatorController(LoadSimulatorService simulator)
    {
        _simulator = simulator;
    }

    [HttpPost("start")]
    public IActionResult Start([FromQuery] int operationsPerSecond = 1)
    {
        _simulator.Start(operationsPerSecond);
        return Ok(new { message = "Load simulator started", operationsPerSecond });
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        _simulator.Stop();
        return Ok(new { message = "Load simulator stopped" });
    }
}
