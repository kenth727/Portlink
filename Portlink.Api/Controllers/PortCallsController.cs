using PortlinkApp.Api.Dtos;
using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Mappers;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace PortlinkApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortCallsController : ControllerBase
{
    private readonly IPortCallRepository _repository;
    private readonly IHubContext<PortOperationsHub> _hubContext;

    public PortCallsController(IPortCallRepository repository, IHubContext<PortOperationsHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] PortCallStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var portCalls = await _repository.GetAllAsync(status, pageNumber, pageSize);
        var totalCount = await _repository.CountAsync(status);

        return Ok(new
        {
            items = portCalls.Select(pc => pc.ToDto()),
            totalCount,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<PortCallDto>>> GetUpcoming(
        [FromQuery] int hours = 24,
        [FromQuery] int limit = 20)
    {
        var fromDate = DateTime.UtcNow;
        var portCalls = await _repository.GetUpcomingAsync(fromDate, limit);
        return Ok(portCalls.Select(pc => pc.ToDto()));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PortCallDto>>> GetActive()
    {
        var portCalls = await _repository.GetActiveAsync();
        return Ok(portCalls.Select(pc => pc.ToDto()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortCallDto>> GetById(int id)
    {
        var portCall = await _repository.GetByIdAsync(id);
        if (portCall is null)
        {
            return NotFound();
        }
        return Ok(portCall.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = "PortOperator")]
    public async Task<ActionResult<PortCallDto>> Create([FromBody] PortCallRequest request)
    {
        var portCall = request.ToEntity();
        var created = await _repository.AddAsync(portCall);

        // Fetch with navigation properties
        created = await _repository.GetByIdAsync(created.Id);

        await _hubContext.Clients.All.SendAsync("PortCallChanged", created!.ToDto());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "PortOperator")]
    public async Task<ActionResult<PortCallDto>> Update(int id, [FromBody] PortCallRequest request)
    {
        var portCall = request.ToEntity(id);
        await _repository.UpdateAsync(portCall);

        // Fetch updated entity with navigation properties
        var updated = await _repository.GetByIdAsync(id);

        await _hubContext.Clients.All.SendAsync("PortCallChanged", updated!.ToDto());
        return Ok(updated.ToDto());
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "PortOperator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("PortCallDeleted", id);
        return NoContent();
    }
}
