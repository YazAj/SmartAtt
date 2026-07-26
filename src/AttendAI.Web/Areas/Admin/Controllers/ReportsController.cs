using AttendAI.Application.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class ReportsController : AdminControllerBase
{
    private readonly IAttendanceReportingService _reportingService;
    private readonly IAttendanceReportExportService _exportService;

    public ReportsController(
        IAttendanceReportingService reportingService,
        IAttendanceReportExportService exportService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _reportingService = reportingService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
        => View(await _reportingService.GetAdminReportAsync(filter, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
        => ExportFile(await _exportService.ExportAdminAttendanceAsync(filter, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Audit([FromQuery] AttendanceAuditFilter filter, CancellationToken cancellationToken)
        => View(await _reportingService.GetAdminAuditReportAsync(filter, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> AuditExport([FromQuery] AttendanceAuditFilter filter, CancellationToken cancellationToken)
        => ExportFile(await _exportService.ExportAdminAuditAsync(filter, cancellationToken));

    private IActionResult ExportFile(ReportFileExportDto export)
    {
        Response.Headers["Cache-Control"] = "no-store, private";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(export.Content, export.ContentType, export.FileName);
    }
}
