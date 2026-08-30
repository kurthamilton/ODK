using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Admin.Members;

public class SendMemberEmailFormViewModel
{
    [Required]
    [DisplayName("Body")]
    public string BodyHtml { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;
}
