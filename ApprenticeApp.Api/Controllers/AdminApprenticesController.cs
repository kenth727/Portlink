using ApprenticeApp.Api.Models.Admin;
using ApprenticeApp.Core.Entities;
using ApprenticeApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApprenticeApp.Api.Controllers;

[Route("Admin/Apprentices")]
public class AdminApprenticesController : Controller
{
    private readonly IApprenticeRepository _apprenticeRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public AdminApprenticesController(IApprenticeRepository apprenticeRepository, IAssignmentRepository assignmentRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _assignmentRepository = assignmentRepository;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var apprentices = await _apprenticeRepository.GetAllAsync(null, null, 1, 500);
        var list = new List<ApprenticeListItem>();

        foreach (var a in apprentices.OrderBy(a => a.LastName).ThenBy(a => a.FirstName))
        {
            var assignmentCount = await _assignmentRepository.CountByApprenticeAsync(a.Id);
            list.Add(new ApprenticeListItem
            {
                Id = a.Id,
                Name = $"{a.FirstName} {a.LastName}",
                Track = a.Track.ToString(),
                Status = a.Status.ToString(),
                AssignmentCount = assignmentCount
            });
        }

        return View(list);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new ApprenticeFormModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApprenticeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var apprentice = new Apprentice
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            StartDate = model.StartDate,
            Track = model.Track,
            Status = model.Status
        };

        await _apprenticeRepository.AddAsync(apprentice);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var apprentice = await _apprenticeRepository.GetByIdAsync(id);
        if (apprentice is null)
        {
            return NotFound();
        }

        var model = new ApprenticeFormModel
        {
            FirstName = apprentice.FirstName,
            LastName = apprentice.LastName,
            Email = apprentice.Email,
            StartDate = apprentice.StartDate,
            Track = apprentice.Track,
            Status = apprentice.Status
        };

        ViewBag.ApprenticeId = apprentice.Id;
        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApprenticeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ApprenticeId = id;
            return View(model);
        }

        var apprentice = await _apprenticeRepository.GetByIdAsync(id);
        if (apprentice is null)
        {
            return NotFound();
        }

        apprentice.FirstName = model.FirstName;
        apprentice.LastName = model.LastName;
        apprentice.Email = model.Email;
        apprentice.StartDate = model.StartDate;
        apprentice.Track = model.Track;
        apprentice.Status = model.Status;

        await _apprenticeRepository.UpdateAsync(apprentice);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _apprenticeRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
