using System.Security.Claims;
using AttendAI.Application.Identity;
using AttendAI.Application.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendAI.Web.Controllers;

[Authorize]
public sealed class ReportsController : Controller
{
    private readonly IAttendanceReportingService _reportingService;
    private readonly IAttendanceReportExportService _exportService;

    public ReportsController(
        IAttendanceReportingService reportingService,
        IAttendanceReportExportService exportService)
    {
        _reportingService = reportingService;
        _exportService = exportService;
    }

    [Authorize(Roles = RoleConstants.Student)]
    [HttpGet]
    public async Task<IActionResult> Student([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetStudentReportAsync(CurrentUserId(), filter, cancellationToken);
        return report is null ? Forbid() : View(report);
    }

    [Authorize(Roles = RoleConstants.Student)]
    [HttpGet]
    public async Task<IActionResult> StudentExport([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
    {
        var export = await _exportService.ExportStudentAttendanceAsync(CurrentUserId(), filter, cancellationToken);
        return export is null ? Forbid() : ExportFile(export);
    }

    [Authorize(Roles = RoleConstants.Instructor)]
    [HttpGet]
    public async Task<IActionResult> Instructor([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
    {
        var report = await _reportingService.GetInstructorReportAsync(CurrentUserId(), filter, cancellationToken);
        return report is null ? Forbid() : View(report);
    }

    [Authorize(Roles = RoleConstants.Instructor)]
    [HttpGet]
    public async Task<IActionResult> InstructorExport([FromQuery] AttendanceReportFilter filter, CancellationToken cancellationToken)
    {
        var export = await _exportService.ExportInstructorAttendanceAsync(CurrentUserId(), filter, cancellationToken);
        return export is null ? Forbid() : ExportFile(export);
    }

    private IActionResult ExportFile(ReportFileExportDto export)
    {
        Response.Headers["Cache-Control"] = "no-store, private";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(export.Content, export.ContentType, export.FileName);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
