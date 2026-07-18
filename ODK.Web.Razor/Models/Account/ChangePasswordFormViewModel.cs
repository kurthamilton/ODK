using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Models.Account;

public class ChangePasswordFormViewModel
{
    [Required]
    [DisplayName("Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string? ConfirmNewPassword { get; set; }

    [Required]
    [DisplayName("Current password")]
    public string? CurrentPassword { get; set; }

    [Required]
    [DisplayName("New password")]
    [PasswordLength]
    public string? NewPassword { get; set; }
}
