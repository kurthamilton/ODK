using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Core.Subscriptions;

public class SiteSubscription : IDatabaseEntity
{
    public const int DefaultGroupLimit = 1;

    public bool Default { get; set; }

    public string DescriptionHtml { get; set; } = string.Empty;

    public int? DisplayOrder { get; set; }

    public bool Enabled { get; set; }

    public EnvironmentType Environment { get; set; }

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

    public PaymentProviderType PaymentProvider { get; set; }

    public PlatformType Platform { get; set; }

    public Guid SitePaymentProductId { get; set; }

    public bool HasCapacity(int memberCount) => RemainingCapacity(memberCount) != 0;

    /// <summary>
    /// Whether the subscription is usable: enabled, and either free or something a member can buy. A free
    /// subscription does not need payments to be enabled - it takes no money, so a payment provider being
    /// off does not stop a member being on it.
    /// </summary>
    public bool IsActive(IEnumerable<SiteSubscriptionPrice> prices)
        => Enabled && (Free || prices.Any());

    /// <summary>
    /// How many more members the group can take, or null when the plan sets no limit. Clamped at zero: a
    /// group can hold more members than its plan allows, because a plan can be downgraded under a group
    /// that is already fuller than the new limit.
    /// </summary>
    public uint? RemainingCapacity(int memberCount) => MemberLimit != null
        ? (uint)Math.Max(0, MemberLimit.Value - memberCount)
        : null;

    public string ToReference() => $"Subscription: {Name}";
}