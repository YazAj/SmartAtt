namespace AttendAI.Application.Identity;

public sealed class DashboardRouteService : IDashboardRouteService
{
    public DashboardDestination GetDashboardForRoles(IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(RoleConstants.Admin))
        {
            return new DashboardDestination("Dashboard", "Admin");
        }

        if (roleSet.Contains(RoleConstants.Instructor))
        {
            return new DashboardDestination("Dashboard", "Instructor");
        }

        if (roleSet.Contains(RoleConstants.Student))
        {
            return new DashboardDestination("Dashboard", "Student");
        }

        return new DashboardDestination("Home", "Index");
    }
}
