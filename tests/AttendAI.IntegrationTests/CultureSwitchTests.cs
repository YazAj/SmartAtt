using System.Net;
using System.Text.RegularExpressions;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AttendAI.IntegrationTests;

public sealed class CultureSwitchTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private static readonly Regex AntiforgeryTokenRegex = new(
        "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly AttendAiWebApplicationFactory _factory;

    public CultureSwitchTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Culture_switch_sets_cookie_and_redirects_to_local_return_url()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var (token, cookieHeader) = await GetAntiforgeryAsync(client);

        using var request = CreateCultureRequest(token, cookieHeader, "ar-JO", "/Account/Login");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.Contains(".AspNetCore.Culture", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Culture_switch_rejects_external_return_url()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var (token, cookieHeader) = await GetAntiforgeryAsync(client);

        using var request = CreateCultureRequest(token, cookieHeader, "en-US", "https://example.com/phish");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    private static async Task<(string Token, string CookieHeader)> GetAntiforgeryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        var tokenMatch = AntiforgeryTokenRegex.Match(html);
        Assert.True(tokenMatch.Success);

        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0]);

        return (tokenMatch.Groups[1].Value, string.Join("; ", cookies));
    }

    private static HttpRequestMessage CreateCultureRequest(
        string token,
        string cookieHeader,
        string culture,
        string returnUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/Preferences/SetCulture")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["culture"] = culture,
                ["returnUrl"] = returnUrl
            })
        };
        request.Headers.Add("Cookie", cookieHeader);
        return request;
    }
}
