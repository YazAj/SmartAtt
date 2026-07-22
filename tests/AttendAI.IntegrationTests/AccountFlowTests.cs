using System.Net;
using System.Text.RegularExpressions;
using AttendAI.Application.Identity;
using AttendAI.Infrastructure.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class AccountFlowTests : IClassFixture<IdentityCookieWebApplicationFactory>
{
    private const string InitialPassword = "Start!12345";
    private static readonly Regex AntiforgeryTokenRegex = new(
        "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly IdentityCookieWebApplicationFactory _factory;

    public AccountFlowTests(IdentityCookieWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_with_valid_credentials_redirects_to_role_dashboard()
    {
        var email = await CreateUserAsync(RoleConstants.Admin);
        var client = CreateCookieClient();
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");

        var response = await PostFormAsync(client, "/Account/Login", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = InitialPassword,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Dashboard/Admin", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.Contains("__Host-AttendAI.Auth", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Authenticated_user_can_change_password()
    {
        var email = await CreateUserAsync(RoleConstants.Admin);
        var client = CreateCookieClient();
        await LoginAsync(client, email);
        var token = await GetAntiforgeryTokenAsync(client, "/Account/ChangePassword");

        var response = await PostFormAsync(client, "/Account/ChangePassword", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["CurrentPassword"] = InitialPassword,
            ["NewPassword"] = "Changed!12345",
            ["ConfirmPassword"] = "Changed!12345"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Profile", response.Headers.Location?.OriginalString);
        Assert.True(await PasswordIsValidAsync(email, "Changed!12345"));
    }

    [Fact]
    public async Task Authenticated_user_can_logout()
    {
        var email = await CreateUserAsync(RoleConstants.Admin);
        var client = CreateCookieClient();
        await LoginAsync(client, email);
        var token = await GetAntiforgeryTokenAsync(client, "/Dashboard/Admin");

        var response = await PostFormAsync(client, "/Account/Logout", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    private HttpClient CreateCookieClient()
        => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

    private async Task<string> CreateUserAsync(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"user-{Guid.NewGuid():N}@attendai.local";
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Integration Test User"
        };

        var createResult = await userManager.CreateAsync(user, InitialPassword);
        Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, role);
        Assert.True(roleResult.Succeeded, string.Join("; ", roleResult.Errors.Select(error => error.Description)));

        return email;
    }

    private async Task LoginAsync(HttpClient client, string email)
    {
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");
        var response = await PostFormAsync(client, "/Account/Login", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = InitialPassword,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private async Task<bool> PasswordIsValidAsync(string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        return await userManager.CheckPasswordAsync(user, password);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var tokenMatch = AntiforgeryTokenRegex.Match(html);
        Assert.True(tokenMatch.Success);

        return tokenMatch.Groups[1].Value;
    }

    private static Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string path,
        Dictionary<string, string> formFields)
        => client.PostAsync(path, new FormUrlEncodedContent(formFields));
}
