namespace ODK.Core.Members;

public class MemberSubscriptionRecord : IDatabaseEntity
{
    public decimal Amount { get; set; }

    public DateTime? CancelledUtc { get; set; }

    public Guid ChapterId { get; set; }

    public Guid? ChapterSubscriptionId { get; set; }

    /// <summary>
    /// The subscription's expiry resulting from this record. Set once at insert (immutable): the first
    /// purchase starts from "now", an extension adds onto the previous record's expiry.
    /// </summary>
    public DateTime? ExpiresUtc { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public string? InitiatorId { get; set; }

    /// <summary>
    /// Marks the member's current record for the chapter (the latest one), so current state can be read
    /// with a single filtered lookup. A denormalised cache of "latest", not an integrity constraint.
    /// </summary>
    public bool IsCurrent { get; set; }

    public Guid MemberId { get; set; }

    public int Months { get; set; }

    public Guid? PaymentId { get; set; }

    public DateTime PurchasedUtc { get; set; }

    public SubscriptionType Type { get; set; }
}
