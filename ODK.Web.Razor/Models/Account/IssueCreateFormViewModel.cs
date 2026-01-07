using System.ComponentModel.DataAnnotations;
using ODK.Core.Issues;

namespace ODK.Web.Razor.Models.Account;

public class IssueCreateFormViewModel
{
    [Required]
    public string? Message { get; set; } = string.Empty;

    [Required]
    public string? Title { get; set; } = string.Empty;

    [Required]
    public IssueType? Type { get; set; }
}
