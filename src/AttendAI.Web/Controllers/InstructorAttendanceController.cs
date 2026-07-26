using System.Security.Claims;
using AttendAI.Application.Attendance;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendAI.Web.Controllers;

[Authorize(Roles = RoleConstants.Instructor)]
public sealed class InstructorAttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;

    public InstructorAttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Roster(Guid id, CancellationToken cancellationToken)
    {
        var roster = await _attendanceService.GetInstructorRosterAsync(CurrentUserId(), id, cancellationToken);
        return roster is null ? Forbid() : View(roster);
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
