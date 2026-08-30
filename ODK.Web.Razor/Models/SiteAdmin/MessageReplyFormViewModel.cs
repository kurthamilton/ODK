using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class MessageReplyFormViewModel
{
    [Required]
    [DisplayName("Message")]
    public string? MessageHtml { get; set; }
}