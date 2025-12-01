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
[Authorize] // All endpoints require authentication
public class VesselsController : ControllerBase
{
    private readonly IVesselRepository _repository;
    private readonly IHubContext<PortOperationsHub> _hubContext;

    public VesselsController(IVesselRepository repository, IHubContext<PortOperationsHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] VesselStatus? status,
        [FromQuery] VesselType? vesselType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var vessels = await _repository.GetAllAsync(status, vesselType, pageNumber, pageSize);
        var totalCount = await _repository.CountAsync(status, vesselType);

        return Ok(new
        {
            items = vessels.Select(v => v.ToDto()),
            totalCount,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VesselDto>> GetById(int id)
    {
        var vessel = await _repository.GetByIdAsync(id);
        if (vessel is null)
        {
            return NotFound();
        }
        return Ok(vessel.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<VesselDto>> Create([FromBody] VesselRequest request)
    {
        try
        {
            var vessel = request.ToEntity();
            var created = await _repository.AddAsync(vessel);
            await _hubContext.Clients.All.SendAsync("VesselChanged", created.ToDto());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A vessel with this IMO number already exists." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VesselDto>> Update(int id, [FromBody] VesselRequest request)
    {
        var vessel = request.ToEntity(id);
        await _repository.UpdateAsync(vessel);
        await _hubContext.Clients.All.SendAsync("VesselChanged", vessel.ToDto());
        return Ok(vessel.ToDto());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("VesselDeleted", id);
        return NoContent();
    }
}
