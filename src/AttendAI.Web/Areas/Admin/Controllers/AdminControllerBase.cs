using AttendAI.Application.Common.Models;
using AttendAI.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleConstants.Admin)]
[AutoValidateAntiforgeryToken]
public abstract class AdminControllerBase : Controller
{
    protected AdminControllerBase(IStringLocalizer<SharedResource> localizer)
    {
        Localizer = localizer;
    }

    protected IStringLocalizer<SharedResource> Localizer { get; }

    protected void AddErrors(OperationResult result)
    {
        foreach (var error in result.Errors)
        {
            var message = error.Arguments is { Length: > 0 }
                ? Localizer[error.MessageKey, error.Arguments].Value
                : Localizer[error.MessageKey].Value;

            ModelState.AddModelError(error.FieldName ?? string.Empty, message);
        }
    }

    protected void FlashSuccess(string resourceKey)
        => TempData["StatusMessage"] = Localizer[resourceKey].Value;

    protected void FlashFailure(OperationResult result)
    {
        var firstError = result.Errors.FirstOrDefault();
        TempData["ErrorMessage"] = firstError is null
            ? Localizer["ErrorRequestFailed"].Value
            : firstError.Arguments is { Length: > 0 }
                ? Localizer[firstError.MessageKey, firstError.Arguments].Value
                : Localizer[firstError.MessageKey].Value;
    }
}
