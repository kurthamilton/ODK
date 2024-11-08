namespace ODK.Core.Payments;

public class PaymentCheckoutSession : IDatabaseEntity
{
    public DateTime? CompletedUtc { get; set; }

    public string SessionId { get; set; } = "";

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid PaymentId { get; set; }

    public DateTime StartedUtc { get; set; }
}
