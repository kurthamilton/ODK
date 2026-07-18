namespace ODK.Core.Members;

public class MemberSubscriptionRecord : IDatabaseEntity
{
    public decimal Amount { get; set; }

    public DateTime? CancelledUtc { get; set; }

    public Guid ChapterId { get; set; }

    public Guid? ChapterSubscriptionId { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public string? InitiatorId { get; set; }

    public Guid MemberId { get; set; }

    public int Months { get; set; }

    public Guid? PaymentId { get; set; }

    public DateTime PurchasedUtc { get; set; }

    public SubscriptionType Type { get; set; }
}
