using AttendAI.Application.Security;

namespace AttendAI.UnitTests.Application;

public sealed class SafeRedirectServiceTests
{
    private readonly SafeRedirectService _service = new();

    [Theory]
    [InlineData("/Dashboard/Admin")]
    [InlineData("~/Dashboard/Admin")]
    public void IsLocalReturnUrl_accepts_local_urls(string returnUrl)
    {
        Assert.True(_service.IsLocalReturnUrl(returnUrl));
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("//example.com")]
    [InlineData("/\\example")]
    [InlineData("")]
    public void IsLocalReturnUrl_rejects_open_redirect_urls(string returnUrl)
    {
        Assert.False(_service.IsLocalReturnUrl(returnUrl));
    }

    [Fact]
    public void NormalizeLocalReturnUrl_returns_fallback_for_invalid_url()
    {
        Assert.Equal("/", _service.NormalizeLocalReturnUrl("https://example.com"));
    }
}
