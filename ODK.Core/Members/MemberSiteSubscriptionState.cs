namespace ODK.Core.Members;

/// <summary>
/// A member's current site subscription - its plan, price, expiry and (for a recurring subscription) its
/// external id and cancellation - read from the current <see cref="MemberSiteSubscriptionRecord"/>
/// (MemberSiteSubscriptionLog). This is the go-forward read model for site-subscription status/feature
/// decisions, replacing reads of the legacy MemberSiteSubscription snapshot.
/// </summary>
public class MemberSiteSubscriptionState
{
    public DateTime? CancelledUtc { get; init; }

    /// <summary>A null expiry is a non-expiring (free/default) subscription - always active.</summary>
    public DateTime? ExpiresUtc { get; init; }

    public string? ExternalId { get; init; }

    public Guid MemberId { get; init; }

    public Guid SiteSubscriptionId { get; init; }

    public Guid? SiteSubscriptionPriceId { get; init; }

    public bool IsActive() => ExpiresUtc == null || ExpiresUtc > DateTime.UtcNow;

    public bool IsExpired() => ExpiresUtc < DateTime.UtcNow;
}
