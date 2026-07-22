using System.Security.Claims;
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
public sealed class FaceVerificationController : Controller
{
    private readonly IFaceVerificationQueryService _queryService;
    private readonly IFaceVerificationService _verificationService;
    private readonly FaceVerificationOptions _options;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public FaceVerificationController(
        IFaceVerificationQueryService queryService,
        IFaceVerificationService verificationService,
        IOptions<FaceVerificationOptions> options,
        IStringLocalizer<SharedResource> localizer)
    {
        _queryService = queryService;
        _verificationService = verificationService;
        _options = options.Value;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _queryService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Verify(CancellationToken cancellationToken)
    {
        PopulateOptions();
        return View(await _queryService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
    }

    [HttpPost]
    [ActionName(nameof(Verify))]
    [RequestFormLimits(MultipartBodyLengthLimit = 2000000)]
    public async Task<IActionResult> VerifyPost(CancellationToken cancellationToken)
    {
        PopulateOptions();
        var file = Request.Form.Files.GetFile("capture");
        if (Request.Form.Files.Any(item => !string.Equals(item.Name, "capture", StringComparison.Ordinal)))
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationUnexpectedField"]);
        }

        if (file is null)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationCaptureRequired"]);
        }
        else if (file.Length > _options.MaximumCaptureBytes || Request.Form.Files.Sum(item => item.Length) > _options.MaximumVerificationRequestBytes)
        {
            ModelState.AddModelError(string.Empty, _localizer["ErrorFaceVerificationRequestTooLarge"]);
        }

        if (!ModelState.IsValid || file is null)
        {
            return View(await _queryService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var result = await _verificationService.VerifyCurrentStudentAsync(
            CurrentUserId(),
            new FaceVerificationCommand(
                new FaceCaptureSample(memory.ToArray(), file.ContentType, file.Name),
                Request.Form["ClientRequestId"].ToString()),
            cancellationToken);

        if (result.Succeeded && result.Value is not null)
        {
            return View("Result", result.Value);
        }

        AddErrors(result);
        return View(await _queryService.GetStudentStatusAsync(CurrentUserId(), cancellationToken));
    }

    private void PopulateOptions()
    {
        ViewBag.MaximumCaptureBytes = _options.MaximumCaptureBytes;
        ViewBag.AllowedContentTypes = string.Join(",", _options.AllowedMimeTypes);
        ViewBag.ClientRequestId = Guid.NewGuid().ToString("N");
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
}
