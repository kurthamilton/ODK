using System.ComponentModel.DataAnnotations;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Models.Account;

public class ActivateFormViewModel
{
    public Guid? ChapterId { get; init; }

    [Required]
    [PasswordLength]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}