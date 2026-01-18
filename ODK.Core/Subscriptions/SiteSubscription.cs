using ODK.Core.Features;
using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Core.Subscriptions;

public class SiteSubscription : IDatabaseEntity
{
    public const int DefaultGroupLimit = 1;

    public bool Default { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? DisplayOrder { get; set; }

    public bool Enabled { get; set; }

    public string? ExternalProductId { get; set; }

    public Guid? FallbackSiteSubscriptionId { get; set; }

    public virtual ICollection<SiteSubscriptionFeature> Features { get; set; } = null!;

    public int? GroupLimit { get; set; }

    public Guid Id { get; set; }

    public int? MemberLimit { get; set; }

    public string Name { get; set; } = string.Empty;

    public PlatformType Platform { get; set; }

    public Guid SitePaymentSettingId { get; set; }

    public bool HasCapacity(int memberCount) => MemberLimit == null || memberCount < MemberLimit;

    public bool HasFeature(SiteFeatureType feature) => Features.Any(x => x.Feature == feature);

    public bool IsEnabled(SitePaymentSettings sitePaymentSettings) => Enabled && sitePaymentSettings.Enabled;

    public string ToReference() => $"Subscription: {Name}";
}