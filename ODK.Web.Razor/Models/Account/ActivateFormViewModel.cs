using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class ActivateFormViewModel
{
    public Guid? ChapterId { get; init; }

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}