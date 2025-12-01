using PortlinkApp.Api.Dtos;
using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Mappers;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BerthsController : ControllerBase
{
    private readonly IBerthRepository _repository;
    private readonly IHubContext<PortOperationsHub> _hubContext;

    public BerthsController(IBerthRepository repository, IHubContext<PortOperationsHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] BerthStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var berths = await _repository.GetAllAsync(status, pageNumber, pageSize);
        var totalCount = await _repository.CountAsync(status);

        return Ok(new
        {
            items = berths.Select(b => b.ToDto()),
            totalCount,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BerthDto>>> GetAvailable()
    {
        var berths = await _repository.GetAvailableBerthsAsync();
        return Ok(berths.Select(b => b.ToDto()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BerthDto>> GetById(int id)
    {
        var berth = await _repository.GetByIdAsync(id);
        if (berth is null)
        {
            return NotFound();
        }
        return Ok(berth.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<BerthDto>> Create([FromBody] BerthRequest request)
    {
        try
        {
            var berth = request.ToEntity();
            var created = await _repository.AddAsync(berth);
            await _hubContext.Clients.All.SendAsync("BerthChanged", created.ToDto());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A berth with this code already exists." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BerthDto>> Update(int id, [FromBody] BerthRequest request)
    {
        var berth = request.ToEntity(id);
        await _repository.UpdateAsync(berth);
        await _hubContext.Clients.All.SendAsync("BerthChanged", berth.ToDto());
        return Ok(berth.ToDto());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("BerthDeleted", id);
        return NoContent();
    }
}
