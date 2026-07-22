using System.Security.Claims;
using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace AttendAI.Web.Controllers;

[Authorize(Roles = RoleConstants.Student)]
[AutoValidateAntiforgeryToken]
public sealed class BiometricEnrollmentController : Controller
{
    private readonly IBiometricEnrollmentService _biometricEnrollmentService;
    private readonly BiometricEnrollmentOptions _options;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public BiometricEnrollmentController(
        IBiometricEnrollmentService biometricEnrollmentService,
        IOptions<BiometricEnrollmentOptions> options,
        IStringLocalizer<SharedResource> localizer)
    {
        _biometricEnrollmentService = biometricEnrollmentService;
        _options = options.Value;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Events = await _biometricEnrollmentService.GetStudentEventsAsync(CurrentUserId(), cancellationToken);
        return View(await _biometricEnrollmentService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Privacy(CancellationToken cancellationToken)
        => View(await _biometricEnrollmentService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> AcceptConsent(CancellationToken cancellationToken)
    {
        var result = await _biometricEnrollmentService.AcceptConsentAsync(CurrentUserId(), cancellationToken);
        if (result.Succeeded)
        {
            FlashSuccess("BiometricConsentAccepted");
            return RedirectToAction(nameof(Enroll));
        }

        FlashFailure(result);
        return RedirectToAction(nameof(Privacy));
    }

    [HttpGet]
    public async Task<IActionResult> Enroll(CancellationToken cancellationToken)
    {
        PopulateOptions();
        return View(await _biometricEnrollmentService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
    }

    [HttpPost]
    [ActionName(nameof(Enroll))]
    [RequestFormLimits(MultipartBodyLengthLimit = 6000000)]
    public async Task<IActionResult> EnrollPost(CancellationToken cancellationToken)
    {
        var files = Request.Form.Files.GetFiles("captures");
        if (Request.Form.Files.Any(file => !string.Equals(file.Name, "captures", StringComparison.Ordinal)))
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorBiometricUnexpectedField"]);
        }

        var totalBytes = files.Sum(file => file.Length);
        if (totalBytes > _options.MaximumEnrollmentRequestBytes)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorBiometricRequestTooLarge"]);
        }

        var captures = new List<FaceCaptureSample>();
        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            captures.Add(new FaceCaptureSample(memory.ToArray(), file.ContentType, file.Name));
        }

        if (ModelState.IsValid)
        {
            var result = await _biometricEnrollmentService.EnrollAsync(
                CurrentUserId(),
                new FaceEnrollmentCommand(captures),
                cancellationToken);

            if (result.Succeeded)
            {
                FlashSuccess("BiometricEnrollmentSucceeded");
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        PopulateOptions();
        return View(await _biometricEnrollmentService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> WithdrawConsent(CancellationToken cancellationToken)
    {
        var result = await _biometricEnrollmentService.WithdrawConsentAsync(CurrentUserId(), cancellationToken);
        if (result.Succeeded)
        {
            FlashSuccess("BiometricConsentWithdrawn");
        }
        else
        {
            FlashFailure(result);
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateOptions()
    {
        ViewBag.RequiredCaptureCount = _options.RequiredCaptureCount;
        ViewBag.MaximumCaptureBytes = _options.MaximumCaptureBytes;
        ViewBag.AllowedContentTypes = string.Join(",", _options.AllowedMimeTypes);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private void AddErrors(OperationResult result)
    {
        foreach (var error in result.Errors)
        {
            var message = error.Arguments is { Length: > 0 }
                ? _localizer[error.MessageKey, error.Arguments].Value
                : _localizer[error.MessageKey].Value;

            ModelState.AddModelError(error.FieldName ?? string.Empty, message);
        }
    }

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
