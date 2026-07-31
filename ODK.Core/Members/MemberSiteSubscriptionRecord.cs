namespace ODK.Core.Members;

public class MemberSiteSubscriptionRecord : IDatabaseEntity
{
    public DateTime? CancelledUtc { get; set; }

    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// The subscription's expiry resulting from this record. Set once at insert (immutable): a first
    /// purchase starts from "now", a renewal adds onto the previous record's expiry. Null for a free /
    /// default subscription that never expires.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public string? InitiatorId { get; set; }

    /// <summary>
    /// Marks the member's current record (the latest one), so current state can be read with a single
    /// filtered lookup. A denormalised cache of "latest", not an integrity constraint.
    /// </summary>
    public bool IsCurrent { get; set; }

    // Nullable for now: the log has pre-existing (payment-only) rows without a member. Phase 1 leaves those
    // null; the backfill populates them (and new records always set it), before a later migration makes it
    // required.
    public Guid? MemberId { get; set; }

    public Guid? PaymentId { get; set; }

    public Guid? SiteSubscriptionPriceId { get; set; }

    public Guid SiteSubscriptionId { get; set; }
}
