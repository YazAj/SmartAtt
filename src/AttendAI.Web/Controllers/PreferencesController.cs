using AttendAI.Application.Security;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AttendAI.Web.Controllers;

public sealed class PreferencesController : Controller
{
    private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
    private readonly ISafeRedirectService _safeRedirectService;

    public PreferencesController(
        IOptions<RequestLocalizationOptions> localizationOptions,
        ISafeRedirectService safeRedirectService)
    {
        _localizationOptions = localizationOptions;
        _safeRedirectService = safeRedirectService;
    }

    [HttpPost]
    public IActionResult SetCulture(string culture, string returnUrl = "/")
    {
        var supportedCulture = _localizationOptions.Value.SupportedUICultures?
            .FirstOrDefault(item => item.Name.Equals(culture, StringComparison.OrdinalIgnoreCase));

        if (supportedCulture is not null)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(supportedCulture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
        }

        return LocalRedirect(_safeRedirectService.NormalizeLocalReturnUrl(returnUrl, "/"));
    }
}
