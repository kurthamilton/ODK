using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class ChangeEmailFormViewModel
{
    public string? Current { get; set; }

    [Required]
    [Display(Name = "New email address")]
    public string? Email { get; set; }
}
