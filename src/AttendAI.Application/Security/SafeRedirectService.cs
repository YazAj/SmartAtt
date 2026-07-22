namespace AttendAI.Application.Security;

public sealed class SafeRedirectService : ISafeRedirectService
{
    public bool IsLocalReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        if (returnUrl[0] == '/')
        {
            return returnUrl.Length == 1 ||
                (returnUrl[1] != '/' && returnUrl[1] != '\\');
        }

        return returnUrl.StartsWith("~/", StringComparison.Ordinal);
    }

    public string NormalizeLocalReturnUrl(string? returnUrl, string fallback = "/")
        => IsLocalReturnUrl(returnUrl) ? returnUrl! : fallback;
}
