using System.Security.Claims;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Identity;
using AttendAI.Application.Lectures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Controllers;

[Authorize(Roles = RoleConstants.Instructor)]
[AutoValidateAntiforgeryToken]
public sealed class InstructorScheduleController : Controller
{
    private readonly ILectureSessionService _lectureSessionService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public InstructorScheduleController(
        ILectureSessionService lectureSessionService,
        IStringLocalizer<SharedResource> localizer)
    {
        _lectureSessionService = lectureSessionService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        return View(await _lectureSessionService.GetInstructorScheduleAsync(userId, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Active(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _lectureSessionService.GetInstructorScheduleAsync(CurrentUserId(), cancellationToken);
        if (!schedule.Any(item => item.ActiveSessionId == id))
        {
            return Forbid();
        }

        var session = await _lectureSessionService.GetByIdAsync(id, includeSensitiveCodeState: true, cancellationToken);
        return session is null ? NotFound() : View(session);
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid scheduleId, CancellationToken cancellationToken)
    {
        var result = await _lectureSessionService.StartAsync(
            CurrentUserId(),
            new StartLectureSessionCommand { LectureScheduleId = scheduleId },
            adminOverride: false,
            cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            FlashFailure(result);
            return RedirectToAction(nameof(Index));
        }

        var session = await _lectureSessionService.GetByIdAsync(result.Value.LectureSessionId, includeSensitiveCodeState: true, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        ViewBag.PlainSessionCode = result.Value.PlainSessionCode;
        ViewBag.SessionCodeExpiresAtLocal = result.Value.SessionCodeExpiresAtLocal;
        return View("Active", session);
    }

    [HttpPost]
    public async Task<IActionResult> RegenerateCode(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lectureSessionService.RegenerateCodeAsync(CurrentUserId(), id, cancellationToken: cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            FlashFailure(result);
            return RedirectToAction(nameof(Active), new { id });
        }

        var session = await _lectureSessionService.GetByIdAsync(id, includeSensitiveCodeState: true, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        ViewBag.PlainSessionCode = result.Value.PlainSessionCode;
        ViewBag.SessionCodeExpiresAtLocal = result.Value.SessionCodeExpiresAtLocal;
        return View("Active", session);
    }

    [HttpPost]
    public async Task<IActionResult> End(Guid id, EndLectureSessionCommand command, CancellationToken cancellationToken)
    {
        var result = await _lectureSessionService.EndAsync(CurrentUserId(), id, command, cancellationToken: cancellationToken);
        if (result.Succeeded) FlashSuccess("LectureSessionEnded"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id, EndLectureSessionCommand command, CancellationToken cancellationToken)
    {
        var result = await _lectureSessionService.CancelAsync(CurrentUserId(), id, command, cancellationToken);
        if (result.Succeeded) FlashSuccess("LectureSessionCancelled"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private void FlashSuccess(string resourceKey)
        => TempData["StatusMessage"] = _localizer[resourceKey].Value;

    private void FlashFailure(OperationResult result)
    {
        var firstError = result.Errors.FirstOrDefault();
        TempData["ErrorMessage"] = firstError is null
            ? _localizer["ErrorRequestFailed"].Value
            : firstError.Arguments is { Length: > 0 }
                ? _localizer[firstError.MessageKey, firstError.Arguments].Value
                : _localizer[firstError.MessageKey].Value;
    }
}
