using AttendAI.Application.Identity;

namespace AttendAI.UnitTests.Application;

public sealed class DashboardRouteServiceTests
{
    private readonly DashboardRouteService _service = new();

    [Theory]
    [InlineData(RoleConstants.Admin, "Admin")]
    [InlineData(RoleConstants.Instructor, "Instructor")]
    [InlineData(RoleConstants.Student, "Student")]
    public void GetDashboardForRoles_routes_known_roles_to_matching_dashboard(string role, string expectedAction)
    {
        var destination = _service.GetDashboardForRoles([role]);

        Assert.Equal("Dashboard", destination.Controller);
        Assert.Equal(expectedAction, destination.Action);
    }

    [Fact]
    public void GetDashboardForRoles_falls_back_to_home_for_unknown_role()
    {
        var destination = _service.GetDashboardForRoles(["Guest"]);

        Assert.Equal("Home", destination.Controller);
        Assert.Equal("Index", destination.Action);
    }
}
