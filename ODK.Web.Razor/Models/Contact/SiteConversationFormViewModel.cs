using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.Contact;

/// <summary>
/// Opening a thread with the site's admins. Separate from <see cref="ConversationFormViewModel"/>, which
/// requires a reCAPTCHA token: only a signed-in member reaches this form, and what protects authed contact
/// is a story of its own. Binding the other model here would fail validation on a token nothing sends.
/// </summary>
public class SiteConversationFormViewModel
{
    [Required]
    public string? Message { get; set; }

    [Required]
    public string? Subject { get; set; }
}
