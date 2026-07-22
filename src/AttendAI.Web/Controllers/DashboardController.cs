using System.Security.Claims;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendAI.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly IDashboardRouteService _dashboardRouteService;

    public DashboardController(IDashboardRouteService dashboardRouteService)
    {
        _dashboardRouteService = dashboardRouteService;
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
    public IActionResult Admin()
    {
        return View();
    }

    [Authorize(Roles = RoleConstants.Instructor)]
    [HttpGet]
    public IActionResult Instructor()
    {
        return View();
    }

    [Authorize(Roles = RoleConstants.Student)]
    [HttpGet]
    public IActionResult Student()
    {
        return View();
    }
}
