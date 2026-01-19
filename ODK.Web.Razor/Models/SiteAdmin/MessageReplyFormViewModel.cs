using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class MessageReplyFormViewModel
{
    [Required]
    public string? Message { get; set; }
}