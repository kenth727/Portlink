using ApprenticeApp.Api.Models.Admin;
using ApprenticeApp.Core.Entities;
using ApprenticeApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApprenticeApp.Api.Controllers;

[Route("Admin/Apprentices/{apprenticeId}/Assignments")]
public class AdminAssignmentsController : Controller
{
    private readonly IApprenticeRepository _apprenticeRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IMentorRepository _mentorRepository;

    public AdminAssignmentsController(
        IApprenticeRepository apprenticeRepository,
        IAssignmentRepository assignmentRepository,
        IMentorRepository mentorRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _assignmentRepository = assignmentRepository;
        _mentorRepository = mentorRepository;
    }

    private async Task<Apprentice?> LoadApprenticeAsync(int apprenticeId) =>
        await _apprenticeRepository.GetByIdAsync(apprenticeId);

    [HttpGet("")]
    public async Task<IActionResult> Index(int apprenticeId)
    {
        var apprentice = await LoadApprenticeAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        var assignments = (await _assignmentRepository.GetByApprenticeAsync(apprenticeId))
            .OrderByDescending(a => a.DueDate)
            .Select(a => new AssignmentListItem
            {
                Id = a.Id,
                Title = a.Title,
                DueDate = a.DueDate,
                Status = a.Status.ToString(),
                MentorName = a.Mentor?.Name ?? string.Empty,
                Notes = a.Notes
            })
            .ToList();

        ViewBag.Apprentice = apprentice;
        return View(assignments);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(int apprenticeId)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        ViewBag.Apprentice = apprentice;
        ViewBag.Mentors = await MentorSelectListAsync();
        return View(new AssignmentFormModel { ApprenticeId = apprenticeId });
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int apprenticeId, int id)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null || assignment.ApprenticeId != apprenticeId)
        {
            return NotFound();
        }

        var model = new AssignmentFormModel
        {
            ApprenticeId = apprenticeId,
            Title = assignment.Title,
            DueDate = assignment.DueDate,
            Status = assignment.Status,
            Notes = assignment.Notes,
            MentorId = assignment.MentorId
        };

        ViewBag.Apprentice = apprentice;
        ViewBag.Mentors = await MentorSelectListAsync();
        ViewBag.AssignmentId = id;
        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int apprenticeId, int id, AssignmentFormModel model)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Apprentice = apprentice;
            ViewBag.Mentors = await MentorSelectListAsync();
            ViewBag.AssignmentId = id;
            return View(model);
        }

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null || assignment.ApprenticeId != apprenticeId)
        {
            return NotFound();
        }

        assignment.Title = model.Title;
        assignment.DueDate = model.DueDate;
        assignment.Status = model.Status;
        assignment.Notes = model.Notes;
        assignment.MentorId = model.MentorId;

        await _assignmentRepository.UpdateAsync(assignment);
        return RedirectToAction(nameof(Index), new { apprenticeId });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int apprenticeId, int id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null || assignment.ApprenticeId != apprenticeId)
        {
            return NotFound();
        }

        await _assignmentRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { apprenticeId });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int apprenticeId, AssignmentFormModel model)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(apprenticeId);
        if (apprentice is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Apprentice = apprentice;
            ViewBag.Mentors = await MentorSelectListAsync();
            return View(model);
        }

        var assignment = new Assignment
        {
            ApprenticeId = apprenticeId,
            MentorId = model.MentorId,
            Title = model.Title,
            DueDate = model.DueDate,
            Status = model.Status,
            Notes = model.Notes
        };

        await _assignmentRepository.AddAsync(assignment);
        return RedirectToAction(nameof(Index), new { apprenticeId });
    }

    private async Task<List<SelectListItem>> MentorSelectListAsync()
    {
        var mentors = await _mentorRepository.GetAllAsync();
        return mentors
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
            .ToList();
    }
}
