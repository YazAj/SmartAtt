using System.Security.Claims;
using AttendAI.Application.Lectures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class LectureSessionsController : AdminControllerBase
{
    private readonly ILectureSessionService _lectureSessionService;

    public LectureSessionsController(
        ILectureSessionService lectureSessionService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _lectureSessionService = lectureSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] LectureSessionQuery query, CancellationToken cancellationToken)
        => View(await _lectureSessionService.GetPagedAsync(query, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var session = await _lectureSessionService.GetByIdAsync(id, includeSensitiveCodeState: true, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        ViewBag.Events = await _lectureSessionService.GetEventsAsync(id, cancellationToken);
        return View(session);
    }

    [HttpPost]
    public async Task<IActionResult> ForceEnd(Guid id, EndLectureSessionCommand command, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _lectureSessionService.EndAsync(userId, id, command, adminForceEnd: true, cancellationToken);
        if (result.Succeeded) FlashSuccess("LectureSessionForceEnded"); else FlashFailure(result);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ExpireStale(CancellationToken cancellationToken)
    {
        var count = await _lectureSessionService.ExpireStaleSessionsAsync(cancellationToken);
        TempData["StatusMessage"] = Localizer["LectureSessionsExpired", count].Value;
        return RedirectToAction(nameof(Index));
    }
}
