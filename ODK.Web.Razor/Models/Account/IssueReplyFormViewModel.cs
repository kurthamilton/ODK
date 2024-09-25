using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Account;

public class IssueReplyFormViewModel
{
    [Required]
    public string? Message { get; set; }
}
