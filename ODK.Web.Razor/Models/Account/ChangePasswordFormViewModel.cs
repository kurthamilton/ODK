using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class ChangePasswordFormViewModel
{
    [Required]
    [DisplayName("Current password")]
    public string? CurrentPassword { get; set; }

    [Required]
    [DisplayName("New password")]
    public string? NewPassword { get; set; }
}
