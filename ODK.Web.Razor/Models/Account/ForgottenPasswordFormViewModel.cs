using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class ForgottenPasswordFormViewModel
{
    public Chapter? Chapter { get; set; }

    [Required]
    [DisplayName("Email address")]
    [EmailAddress]
    public string? EmailAddress { get; set; }
}
