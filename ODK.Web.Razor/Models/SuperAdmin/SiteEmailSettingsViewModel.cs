using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteEmailSettingsViewModel
{
    [DisplayName("Contact Address")]
    [EmailAddress]
    [Required]
    public string ContactEmailAddress { get; set; } = "";

    [DisplayName("From Address")]
    [EmailAddress]
    [Required]
    public string FromEmailAddress { get; set; } = "";

    [DisplayName("From Name")]
    [Required]
    public string FromEmailName { get; set; } = "";

    [Required]
    public string Title { get; set; } = "";
}
