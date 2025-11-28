using ApprenticeApp.Api.Dtos;
using ApprenticeApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApprenticeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentorsController : ControllerBase
{
    private readonly IMentorRepository _mentorRepository;

    public MentorsController(IMentorRepository mentorRepository)
    {
        _mentorRepository = mentorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MentorDto>>> GetMentors()
    {
        var mentors = await _mentorRepository.GetAllAsync();
        return Ok(mentors.Select(m => new MentorDto(m.Id, m.Name, m.Email)));
    }
}
