using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteEmailSettingsViewModel
{
    [DisplayName("Contact Address")]
    [EmailAddress]
    [Required]
    public string ContactEmailAddress { get; set; } = string.Empty;

    [DisplayName("From Address")]
    [EmailAddress]
    [Required]
    public string FromEmailAddress { get; set; } = string.Empty;

    [DisplayName("From Name")]
    [Required]
    public string FromEmailName { get; set; } = string.Empty;

    [DisplayName("Platform Title")]
    [Required]
    public string PlatformTitle { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
}