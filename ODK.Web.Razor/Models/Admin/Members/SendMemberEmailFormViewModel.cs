using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SendMemberEmailFormViewModel
{
    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;
}
