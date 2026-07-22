using AttendAI.Application.FaceVerification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class FaceVerificationsController : AdminControllerBase
{
    private readonly IFaceVerificationQueryService _queryService;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;

    public FaceVerificationsController(
        IFaceVerificationQueryService queryService,
        IFaceEngineDiagnosticsService diagnosticsService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _queryService = queryService;
        _diagnosticsService = diagnosticsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminFaceVerificationAttemptQuery query, CancellationToken cancellationToken)
    {
        ViewBag.Diagnostics = _diagnosticsService.GetDiagnostics();
        return View(await _queryService.GetAdminAttemptsAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await _queryService.GetAdminAttemptDetailsAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public IActionResult Diagnostics()
        => View(_diagnosticsService.GetDiagnostics());
}
