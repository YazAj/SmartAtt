using System.Net;
using AttendAI.Application.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AttendAI.IntegrationTests;

public sealed class BiometricEnrollmentAuthorizationTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private readonly AttendAiWebApplicationFactory _factory;

    public BiometricEnrollmentAuthorizationTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_can_access_biometric_profile_page()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);

        var response = await client.GetAsync("/BiometricEnrollment");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Instructor_cannot_access_student_biometric_profile_page()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Instructor);

        var response = await client.GetAsync("/BiometricEnrollment");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_access_biometric_oversight_page()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Admin);

        var response = await client.GetAsync("/Admin/BiometricEnrollments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleConstants.Student)]
    [InlineData(RoleConstants.Instructor)]
    public async Task Non_admin_roles_cannot_access_biometric_oversight_page(string role)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        var response = await client.GetAsync("/Admin/BiometricEnrollments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_cannot_access_biometric_pages()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var studentResponse = await client.GetAsync("/BiometricEnrollment");
        var adminResponse = await client.GetAsync("/Admin/BiometricEnrollments");

        Assert.Equal(HttpStatusCode.Unauthorized, studentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, adminResponse.StatusCode);
    }
}
