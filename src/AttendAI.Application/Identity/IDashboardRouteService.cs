namespace AttendAI.Application.Identity;

public interface IDashboardRouteService
{
    DashboardDestination GetDashboardForRoles(IEnumerable<string> roles);
}
