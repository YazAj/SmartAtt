using System.Security.Claims;
using AttendAI.Application.Identity;
using AttendAI.Application.Lectures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendAI.Web.Controllers;

[Authorize(Roles = RoleConstants.Student)]
[AutoValidateAntiforgeryToken]
public sealed class StudentScheduleController : Controller
{
    private readonly ILectureSessionService _lectureSessionService;

    public StudentScheduleController(ILectureSessionService lectureSessionService)
    {
        _lectureSessionService = lectureSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _lectureSessionService.GetStudentScheduleAsync(CurrentUserId(), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Active(Guid id, CancellationToken cancellationToken)
    {
        var session = await _lectureSessionService.GetStudentActiveSessionAsync(CurrentUserId(), id, cancellationToken);
        return session is null ? NotFound() : View(session);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
