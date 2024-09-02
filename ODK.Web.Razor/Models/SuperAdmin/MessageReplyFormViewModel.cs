using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class MessageReplyFormViewModel
{
    [Required]
    public string? Message { get; set; }
}
