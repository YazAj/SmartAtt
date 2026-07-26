using System.Net;
using AttendAI.Application.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AttendAI.IntegrationTests;

public sealed class AttendanceAuthorizationTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private readonly AttendAiWebApplicationFactory _factory;

    public AttendanceAuthorizationTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_can_open_attendance_index()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);

        var response = await client.GetAsync("/Attendance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.Instructor)]
    public async Task Non_student_roles_cannot_open_student_attendance(string role)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        var response = await client.GetAsync("/Attendance");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_cannot_open_instructor_attendance_roster()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);

        var response = await client.GetAsync($"/InstructorAttendance/Roster/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_attendance_user_is_challenged()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Attendance");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
