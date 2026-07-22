using AttendAI.Application.Identity;
using AttendAI.Application.Security;
using AttendAI.Infrastructure.Identity;
using AttendAI.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly IDashboardRouteService _dashboardRouteService;
    private readonly ILogger<AccountController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ISafeRedirectService _safeRedirectService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDashboardRouteService dashboardRouteService,
        ISafeRedirectService safeRedirectService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dashboardRouteService = dashboardRouteService;
        _safeRedirectService = safeRedirectService;
        _localizer = localizer;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = _safeRedirectService.NormalizeLocalReturnUrl(returnUrl, "/")
        });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.ReturnUrl = _safeRedirectService.NormalizeLocalReturnUrl(model.ReturnUrl, "/");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, _localizer["InvalidLogin"]);
            return View(model);
        }

        if (user.IsDisabled)
        {
            _logger.LogWarning("Disabled account login attempt for user {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, _localizer["AccountDisabled"]);
            return View(model);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            user.LastLoginAtUtc = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            if (_safeRedirectService.IsLocalReturnUrl(model.ReturnUrl) && model.ReturnUrl != "/")
            {
                return LocalRedirect(model.ReturnUrl);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var destination = _dashboardRouteService.GetDashboardForRoles(roles);
            return RedirectToAction(destination.Action, destination.Controller);
        }

        if (signInResult.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, _localizer["AccountLocked"]);
            return View(model);
        }

        ModelState.AddModelError(string.Empty, _localizer["InvalidLogin"]);
        cancellationToken.ThrowIfCancellationRequested();
        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["StatusMessage"] = _localizer["LogoutSuccessful"].Value;
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = _localizer["PasswordChanged"].Value;
        return RedirectToAction(nameof(ProfileController.Index), "Profile");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
