namespace ODK.Core.Payments;

public class PaymentCheckoutSession : IDatabaseEntity, IMemberEntity
{
    public DateTime? CompletedUtc { get; set; }

    public DateTime? ExpiredUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid PaymentId { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public DateTime StartedUtc { get; set; }
}