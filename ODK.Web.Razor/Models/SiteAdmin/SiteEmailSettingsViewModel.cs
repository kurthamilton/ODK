using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteEmailSettingsViewModel
{
    [DisplayName("Admin Title")]
    [Required]
    public string AdminTitle { get; set; } = string.Empty;

    [DisplayName("From Address")]
    [EmailAddress]
    [Required]
    public string FromEmailAddress { get; set; } = string.Empty;

    [DisplayName("From Name")]
    [Required]
    public string FromEmailName { get; set; } = string.Empty;

    [DisplayName("Member Title")]
    [Required]
    public string MemberTitle { get; set; } = string.Empty;

    [DisplayName("Platform Title")]
    [Required]
    public string PlatformTitle { get; set; } = string.Empty;
}