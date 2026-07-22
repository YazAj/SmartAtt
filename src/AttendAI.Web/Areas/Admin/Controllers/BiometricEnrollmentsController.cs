using System.Security.Claims;
using AttendAI.Application.Biometrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class BiometricEnrollmentsController : AdminControllerBase
{
    private readonly IBiometricEnrollmentService _biometricEnrollmentService;
    private readonly IFaceEngineReadinessService _readinessService;

    public BiometricEnrollmentsController(
        IBiometricEnrollmentService biometricEnrollmentService,
        IFaceEngineReadinessService readinessService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _biometricEnrollmentService = biometricEnrollmentService;
        _readinessService = readinessService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminBiometricEnrollmentQuery query, CancellationToken cancellationToken)
    {
        ViewBag.EngineReadiness = _readinessService.GetReadiness();
        return View(await _biometricEnrollmentService.GetAdminPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        ViewBag.EngineReadiness = _readinessService.GetReadiness();
        var model = await _biometricEnrollmentService.GetAdminDetailsAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Revoke(Guid id, AdminBiometricActionCommand command, CancellationToken cancellationToken)
    {
        var result = await _biometricEnrollmentService.AdminRevokeAsync(id, CurrentUserId(), command, cancellationToken);
        if (result.Succeeded) FlashSuccess("BiometricTemplateRevoked"); else FlashFailure(result);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> RequireReEnrollment(Guid id, AdminBiometricActionCommand command, CancellationToken cancellationToken)
    {
        var result = await _biometricEnrollmentService.AdminRequireReEnrollmentAsync(id, CurrentUserId(), command, cancellationToken);
        if (result.Succeeded) FlashSuccess("BiometricReEnrollmentRequired"); else FlashFailure(result);
        return RedirectToAction(nameof(Details), new { id });
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
