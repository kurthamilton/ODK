using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Features;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.SuperAdmin;

public class SiteSubscriptionFormViewModel
{
    [Required]
    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    [DisplayName("Fallback")]
    public Guid? FallbackSiteSubscriptionId { get; set; }

    public List<SiteFeatureType> Features { get; set; } = new();

    [DisplayName("Group limit")]
    public int? GroupLimit { get; set; }

    [DisplayName("Member limit")]
    public int? MemberLimit { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DisplayName("Site payment settings")]
    public Guid? SitePaymentSettingId { get; set; }

    public IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; set; } = [];

    public Guid? SiteSubscriptionId { get; set; }
}