using ApprenticeApp.Api.Dtos;
using ApprenticeApp.Api.Mappers;
using ApprenticeApp.Api.Requests;
using ApprenticeApp.Api.Hubs;
using ApprenticeApp.Core.Entities;
using ApprenticeApp.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace ApprenticeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprenticesController : ControllerBase
{
    private readonly IApprenticeRepository _apprenticeRepository;
    private readonly IHubContext<ApprenticeHub> _hubContext;

    public ApprenticesController(IApprenticeRepository apprenticeRepository, IHubContext<ApprenticeHub> hubContext)
    {
        _apprenticeRepository = apprenticeRepository;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetApprentices(
        [FromQuery] ApprenticeStatus? status,
        [FromQuery] ApprenticeTrack? track,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var apprentices = await _apprenticeRepository.GetAllAsync(status, track, pageNumber, pageSize);
        var totalCount = await _apprenticeRepository.CountAsync(status, track);

        return Ok(new
        {
            items = apprentices.Select(a => a.ToDto()),
            totalCount,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApprenticeDto>> GetApprentice(int id)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(id);
        if (apprentice is null)
        {
            return NotFound();
        }

        return Ok(apprentice.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ApprenticeDto>> CreateApprentice([FromBody] ApprenticeRequest request)
    {
        var apprentice = request.ToEntity();
        try
        {
            var created = await _apprenticeRepository.AddAsync(apprentice);
            await _hubContext.Clients.All.SendAsync("ApprenticeChanged", new { action = "created", apprentice = created.ToDto() });
            return CreatedAtAction(nameof(GetApprentice), new { id = created.Id }, created.ToDto());
        }
        catch (DbUpdateException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApprenticeDto>> UpdateApprentice(int id, [FromBody] ApprenticeRequest request)
    {
        var existing = await _apprenticeRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.Email = request.Email;
        existing.StartDate = request.StartDate;
        existing.Track = request.Track;
        existing.Status = request.Status;

        try
        {
            await _apprenticeRepository.UpdateAsync(existing);
            await _hubContext.Clients.All.SendAsync("ApprenticeChanged", new { action = "updated", apprentice = existing.ToDto() });
            return Ok(existing.ToDto());
        }
        catch (DbUpdateException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteApprentice(int id)
    {
        var existing = await _apprenticeRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        await _apprenticeRepository.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("ApprenticeChanged", new { action = "deleted", apprenticeId = id });
        return NoContent();
    }
}
