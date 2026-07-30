namespace ODK.Core.Members;

/// <summary>
/// A member's current subscription for a chapter - its type, expiry, and whether it's an active recurring
/// (auto-renewing) subscription - read from the current <see cref="MemberSubscriptionRecord"/>
/// (MemberSubscriptionLog). This is the go-forward read model for subscription status/visibility decisions,
/// replacing reads of the legacy MemberSubscription snapshot.
/// </summary>
public class MemberChapterSubscription
{
    public DateTime? CancelledUtc { get; init; }

    public Guid ChapterId { get; init; }

    public DateTime? ExpiresUtc { get; init; }

    public Guid MemberId { get; init; }

    /// <summary>Whether the subscription this record is for auto-renews (a recurring chapter subscription).</summary>
    public bool Recurring { get; init; }

    public SubscriptionType Type { get; init; }

    /// <summary>
    /// Whether the subscription is recurring and hasn't been cancelled - i.e. it will auto-renew, so no
    /// expiry warning is needed.
    /// </summary>
    public bool IsActiveRecurring() => Recurring && CancelledUtc == null;

    public bool IsExpired() => ExpiresUtc < DateTime.UtcNow;
}
