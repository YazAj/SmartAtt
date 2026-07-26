using System.Net;
using AttendAI.Application.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class ReportingAuthorizationTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private readonly AttendAiWebApplicationFactory _factory;

    public ReportingAuthorizationTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/Reports/Student")]
    [InlineData("/Reports/Instructor")]
    [InlineData("/Admin/Reports")]
    public async Task Anonymous_user_is_challenged_for_reports(string url)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleConstants.Instructor, "/Reports/Student")]
    [InlineData(RoleConstants.Student, "/Reports/Instructor")]
    [InlineData(RoleConstants.Student, "/Admin/Reports")]
    [InlineData(RoleConstants.Instructor, "/Admin/Reports/Audit")]
    public async Task Wrong_role_is_forbidden_for_report_pages(string role, string url)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Role_owners_can_open_their_report_pages()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            await ReportingServiceTests.SeedReportingScenarioAsync(scope, "test-user-id", "test-user-id");
        }

        var studentClient = _factory.CreateClient();
        studentClient.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Student);
        var instructorClient = _factory.CreateClient();
        instructorClient.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Instructor);
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Admin);

        Assert.Equal(HttpStatusCode.OK, (await studentClient.GetAsync("/Reports/Student")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await instructorClient.GetAsync("/Reports/Instructor")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/Admin/Reports")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/Admin/Reports/Audit")).StatusCode);
    }

    [Fact]
    public async Task Admin_csv_export_uses_no_store_headers_and_safe_columns()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", RoleConstants.Admin);

        var response = await client.GetAsync("/Admin/Reports/Export");
        var csv = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Course Code", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("FaceScore", csv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Template", csv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Latitude", csv, StringComparison.OrdinalIgnoreCase);
    }
}
