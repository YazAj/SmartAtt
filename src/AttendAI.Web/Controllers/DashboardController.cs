using System.Security.Claims;
using AttendAI.Application.Academic.Dashboard;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendAI.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly IAcademicDashboardService _academicDashboardService;
    private readonly IDashboardRouteService _dashboardRouteService;

    public DashboardController(
        IDashboardRouteService dashboardRouteService,
        IAcademicDashboardService academicDashboardService)
    {
        _dashboardRouteService = dashboardRouteService;
        _academicDashboardService = academicDashboardService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value);
        var destination = _dashboardRouteService.GetDashboardForRoles(roles);
        return RedirectToAction(destination.Action, destination.Controller);
    }

    [Authorize(Roles = RoleConstants.Admin)]
    [HttpGet]
    public async Task<IActionResult> Admin(CancellationToken cancellationToken)
    {
        var model = await _academicDashboardService.GetAdminSummaryAsync(cancellationToken);
        return View(model);
    }

    [Authorize(Roles = RoleConstants.Instructor)]
    [HttpGet]
    public async Task<IActionResult> Instructor(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var model = await _academicDashboardService.GetInstructorSummaryAsync(userId, cancellationToken);
        return View(model);
    }

    [Authorize(Roles = RoleConstants.Student)]
    [HttpGet]
    public async Task<IActionResult> Student(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var model = await _academicDashboardService.GetStudentSummaryAsync(userId, cancellationToken);
        return View(model);
    }
}
