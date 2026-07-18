using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Models.Account;

public class ResetPasswordFormViewModel
{
    [Required]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "New password")]
    [PasswordLength]
    public string NewPassword { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
