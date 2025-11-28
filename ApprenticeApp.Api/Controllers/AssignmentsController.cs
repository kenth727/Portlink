using ApprenticeApp.Api.Dtos;
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
[Route("api")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IApprenticeRepository _apprenticeRepository;
    private readonly IHubContext<ApprenticeHub> _hubContext;

    public AssignmentsController(IAssignmentRepository assignmentRepository, IApprenticeRepository apprenticeRepository, IHubContext<ApprenticeHub> hubContext)
    {
        _assignmentRepository = assignmentRepository;
        _apprenticeRepository = apprenticeRepository;
        _hubContext = hubContext;
    }

    [HttpGet("apprentices/{apprenticeId:int}/assignments")]
    public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAssignments(int apprenticeId)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        var assignments = await _assignmentRepository.GetByApprenticeAsync(apprenticeId);
        return Ok(assignments.Select(a => new AssignmentDto(
            a.Id,
            a.Title,
            a.DueDate,
            a.Status,
            a.Notes,
            a.Mentor?.Name ?? string.Empty)));
    }

    [HttpPost("apprentices/{apprenticeId:int}/assignments")]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment(int apprenticeId, [FromBody] AssignmentRequest request)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        var assignment = new Assignment
        {
            ApprenticeId = apprenticeId,
            MentorId = request.MentorId,
            Title = request.Title,
            DueDate = request.DueDate,
            Status = request.Status,
            Notes = request.Notes
        };

        try
        {
            var created = await _assignmentRepository.AddAsync(assignment);
            var createdWithMentor = await _assignmentRepository.GetByIdAsync(created.Id) ?? created;
            await _hubContext.Clients.All.SendAsync("AssignmentChanged", new
            {
                action = "created",
                apprenticeId = apprenticeId,
                assignment = new AssignmentDto(
                    createdWithMentor.Id,
                    createdWithMentor.Title,
                    createdWithMentor.DueDate,
                    createdWithMentor.Status,
                    createdWithMentor.Notes,
                    createdWithMentor.Mentor?.Name ?? string.Empty)
            });
            return CreatedAtAction(nameof(GetAssignments), new { apprenticeId }, new AssignmentDto(
                createdWithMentor.Id,
                createdWithMentor.Title,
                createdWithMentor.DueDate,
                createdWithMentor.Status,
                createdWithMentor.Notes,
                createdWithMentor.Mentor?.Name ?? string.Empty));
        }
        catch (DbUpdateException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("assignments/{id:int}")]
    public async Task<ActionResult<AssignmentDto>> UpdateAssignment(int id, [FromBody] AssignmentRequest request)
    {
        var existing = await _assignmentRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = request.Title;
        existing.DueDate = request.DueDate;
        existing.Status = request.Status;
        existing.Notes = request.Notes;
        existing.MentorId = request.MentorId;

        try
        {
            await _assignmentRepository.UpdateAsync(existing);
            var refreshed = await _assignmentRepository.GetByIdAsync(existing.Id) ?? existing;
            await _hubContext.Clients.All.SendAsync("AssignmentChanged", new
            {
                action = "updated",
                apprenticeId = refreshed.ApprenticeId,
                assignment = new AssignmentDto(
                    refreshed.Id,
                    refreshed.Title,
                    refreshed.DueDate,
                    refreshed.Status,
                    refreshed.Notes,
                    refreshed.Mentor?.Name ?? string.Empty)
            });
            return Ok(new AssignmentDto(
                refreshed.Id,
                refreshed.Title,
                refreshed.DueDate,
                refreshed.Status,
                refreshed.Notes,
                refreshed.Mentor?.Name ?? string.Empty));
        }
        catch (DbUpdateException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("assignments/{id:int}")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var existing = await _assignmentRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        await _assignmentRepository.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("AssignmentChanged", new
        {
            action = "deleted",
            apprenticeId = existing.ApprenticeId,
            assignmentId = id
        });
        return NoContent();
    }
}
