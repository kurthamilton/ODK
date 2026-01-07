using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class ResetPasswordFormViewModel
{
    [Required]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
