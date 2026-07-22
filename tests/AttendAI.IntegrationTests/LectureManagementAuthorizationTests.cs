using System.Net;
using System.Text.RegularExpressions;
using AttendAI.Application.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AttendAI.IntegrationTests;

public sealed class LectureManagementAuthorizationTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private static readonly Regex AntiforgeryTokenRegex = new(
        "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly AttendAiWebApplicationFactory _factory;

    public LectureManagementAuthorizationTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_can_access_lecture_management_pages()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Admin);

        var schedules = await client.GetAsync("/Admin/LectureSchedules");
        var sessions = await client.GetAsync("/Admin/LectureSessions");

        Assert.Equal(HttpStatusCode.OK, schedules.StatusCode);
        Assert.Equal(HttpStatusCode.OK, sessions.StatusCode);
    }

    [Theory]
    [InlineData(RoleConstants.Instructor)]
    [InlineData(RoleConstants.Student)]
    public async Task Non_admin_roles_cannot_access_admin_lecture_management(string role)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        var response = await client.GetAsync("/Admin/LectureSchedules");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_cannot_access_admin_lecture_management()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Admin/LectureSchedules");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Instructor_and_student_can_access_their_schedule_pages()
    {
        var instructor = _factory.CreateClient();
        instructor.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Instructor);
        var student = _factory.CreateClient();
        student.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);

        var instructorResponse = await instructor.GetAsync("/InstructorSchedule");
        var studentResponse = await student.GetAsync("/StudentSchedule");

        Assert.Equal(HttpStatusCode.OK, instructorResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, studentResponse.StatusCode);
    }

    [Fact]
    public async Task Student_cannot_start_instructor_session()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);
        var token = await GetAntiforgeryTokenAsync(client, "/Account/ChangePassword");

        var response = await client.PostAsync(
            "/InstructorSchedule/Start?scheduleId=00000000-0000-0000-0000-000000000001",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var match = AntiforgeryTokenRegex.Match(html);
        Assert.True(match.Success);
        return match.Groups[1].Value;
    }
}
