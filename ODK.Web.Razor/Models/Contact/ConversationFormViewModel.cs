using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Contact;

public class ConversationFormViewModel
{
    [Required]
    public string? Message { get; set; }

    [Required]
    public string? Recaptcha { get; set; }

    [Required]
    public string? Subject { get; set; }    
}
