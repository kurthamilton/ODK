using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class ReferFormViewModel
{
    [Display(Name = "Email address")]
    [Required]
    public string EmailAddress { get; set; } = string.Empty;
}
