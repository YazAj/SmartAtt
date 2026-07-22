namespace AttendAI.Application.Security;

public interface ISafeRedirectService
{
    bool IsLocalReturnUrl(string? returnUrl);

    string NormalizeLocalReturnUrl(string? returnUrl, string fallback = "/");
}
