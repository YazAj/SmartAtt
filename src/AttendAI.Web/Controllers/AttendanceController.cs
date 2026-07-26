using System.Globalization;
using System.Security.Claims;
using AttendAI.Application.Attendance;
using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Models;
using AttendAI.Application.FaceVerification;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace AttendAI.Web.Controllers;

[Authorize(Roles = RoleConstants.Student)]
[AutoValidateAntiforgeryToken]
public sealed class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly FaceVerificationOptions _faceVerificationOptions;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AttendanceController(
        IAttendanceService attendanceService,
        IOptions<FaceVerificationOptions> faceVerificationOptions,
        IStringLocalizer<SharedResource> localizer)
    {
        _attendanceService = attendanceService;
        _faceVerificationOptions = faceVerificationOptions.Value;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _attendanceService.GetStudentPageAsync(CurrentUserId(), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken cancellationToken)
    {
        var challenge = await _attendanceService.IssueChallengeAsync(CurrentUserId(), id, cancellationToken);
        if (!challenge.Succeeded || challenge.Value is null)
        {
            FlashFailure(challenge);
            return RedirectToAction(nameof(Index));
        }

        return View(challenge.Value);
    }

    [HttpPost]
    [ActionName(nameof(CheckIn))]
    [RequestFormLimits(MultipartBodyLengthLimit = 2000000)]
    public async Task<IActionResult> CheckInPost(Guid id, CancellationToken cancellationToken)
    {
        var file = Request.Form.Files.GetFile("capture");
        if (Request.Form.Files.Any(item => !string.Equals(item.Name, "capture", StringComparison.Ordinal)))
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationUnexpectedField"]);
        }

        if (file is null)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationCaptureRequired"]);
        }
        else if (file.Length > _faceVerificationOptions.MaximumCaptureBytes ||
                 Request.Form.Files.Sum(item => item.Length) > _faceVerificationOptions.MaximumVerificationRequestBytes)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationRequestTooLarge"]);
        }

        var latitude = ParseDouble("Latitude");
        var longitude = ParseDouble("Longitude");
        var accuracy = ParseDouble("AccuracyMeters");
        var capturedAt = ParseDateTimeOffset("BrowserTimestampUtc");
        if (!latitude.HasValue || !longitude.HasValue || !accuracy.HasValue)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorAttendanceLocationRequired"]);
        }

        if (!ModelState.IsValid || file is null || !latitude.HasValue || !longitude.HasValue || !accuracy.HasValue)
        {
            var newChallenge = await _attendanceService.IssueChallengeAsync(CurrentUserId(), id, cancellationToken);
            return newChallenge.Succeeded && newChallenge.Value is not null
                ? View(newChallenge.Value)
                : RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var result = await _attendanceService.CheckInAsync(
            CurrentUserId(),
            new AttendanceCheckInCommand(
                id,
                Request.Form["ChallengeToken"].ToString(),
                Request.Form["IdempotencyKey"].ToString(),
                new FaceCaptureSample(memory.ToArray(), file.ContentType, file.Name),
                new BrowserLocationSample(latitude.Value, longitude.Value, accuracy.Value, capturedAt)),
            cancellationToken);

        if (result.Succeeded && result.Value is not null)
        {
            return View("Result", result.Value);
        }

        FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private double? ParseDouble(string fieldName)
        => double.TryParse(Request.Form[fieldName].ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private DateTimeOffset? ParseDateTimeOffset(string fieldName)
        => DateTimeOffset.TryParse(Request.Form[fieldName].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value)
            ? value.ToUniversalTime()
            : null;

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
