using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Features;

namespace ODK.Web.Razor.Models.SiteAdmin;

public class SiteSubscriptionFormSubmitViewModel
{
    [Required]
    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    [DisplayName("Fallback")]
    public Guid? FallbackSiteSubscriptionId { get; set; }

    public List<SiteFeatureType>? Features { get; set; }

    [DisplayName("Free (no payment required)")]
    public bool Free { get; set; }

    [DisplayName("Group limit")]
    public int? GroupLimit { get; set; }

    [DisplayName("Member limit")]
    public int? MemberLimit { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
