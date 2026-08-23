namespace ODK.Core.Subscriptions;

/// <summary>
/// How long an expired site subscription keeps the access it paid for. The expiry stored against a
/// subscription is never moved by the cooldown - it widens the window a check treats as active, so the
/// same stored expiry reads as active or expired according to the cooldown in force when it is read.
/// </summary>
public class SiteSubscriptionCooldown
{
    public SiteSubscriptionCooldown(int months)
    {
        // A negative cooldown is meaningless and is treated as none, so it can never narrow the window and
        // make a live subscription read as expired.
        Months = Math.Max(0, months);
    }

    public int Months { get; }

    /// <summary>
    /// The instant an expiry must fall after for the subscription to still count as active - now, less the
    /// cooldown. A subscription that expired at or before it is expired.
    /// </summary>
    public DateTime ActiveAfterUtc(DateTime utcNow) => utcNow.AddMonths(-Months);

    /// <summary>
    /// Whether a subscription expiring at <paramref name="expiresUtc"/> counts as active. A null expiry is a
    /// non-expiring (free/default) subscription - always active.
    /// </summary>
    public bool IsActive(DateTime? expiresUtc, DateTime utcNow)
        => expiresUtc == null || expiresUtc > ActiveAfterUtc(utcNow);
}
