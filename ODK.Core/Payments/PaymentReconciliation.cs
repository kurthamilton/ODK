namespace ODK.Core.Payments;

public class PaymentReconciliation : IDatabaseEntity
{
    public decimal Amount { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public required string PaymentReference { get; set; }
}
