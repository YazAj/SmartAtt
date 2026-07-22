using System.ComponentModel.DataAnnotations;

namespace AttendAI.Web.Models.Account;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "ValidationRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "CurrentPassword")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ValidationRequired")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "ValidationPasswordLength")]
    [DataType(DataType.Password)]
    [Display(Name = "NewPassword")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ValidationRequired")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "ValidationPasswordConfirm")]
    [Display(Name = "ConfirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
