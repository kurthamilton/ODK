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

    /// <summary>
    /// Whether the subscription costs nothing, so it is usable without any price. A free subscription
    /// takes no payment, so it never has a paid price and never expires.
    /// </summary>
    public bool Free { get; set; }

    public int? GroupLimit { get; set; }

    public Guid Id { get; set; }

    public int? MemberLimit { get; set; }

    public string Name { get; set; } = string.Empty;

    public PlatformType Platform { get; set; }

    public Guid? SitePaymentProductId { get; set; }

    public Guid SitePaymentSettingId { get; set; }

    public bool HasCapacity(int memberCount) => MemberLimit == null || memberCount < MemberLimit;

    /// <summary>
    /// Whether the subscription is usable: enabled, and either free or something a member can buy. A free
    /// subscription does not need the payment settings to be enabled - it takes no money, so a payment
    /// provider being off does not stop a member being on it.
    /// </summary>
    public bool IsActive(
        IEnumerable<SiteSubscriptionPrice> prices, SitePaymentSettings sitePaymentSettings)
        => Enabled && (Free || (prices.Any() && sitePaymentSettings.Enabled));

    public string ToReference() => $"Subscription: {Name}";
}