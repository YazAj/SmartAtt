using AttendAI.Application.Identity;

namespace AttendAI.UnitTests.Application;

public sealed class RoleConstantsTests
{
    [Fact]
    public void All_contains_required_sprint_one_roles()
    {
        Assert.Contains(RoleConstants.Admin, RoleConstants.All);
        Assert.Contains(RoleConstants.Instructor, RoleConstants.All);
        Assert.Contains(RoleConstants.Student, RoleConstants.All);
        Assert.Equal(3, RoleConstants.All.Count);
    }
}
