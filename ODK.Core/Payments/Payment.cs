namespace ODK.Core.Payments;

public class Payment : IDatabaseEntity
{
    public Payment()
    {
    }

    public decimal Amount { get; set; }

    public Guid? ChapterId { get; set; }

    public DateTime? CreatedUtc { get; set; }

    public Guid CurrencyId { get; set; }

    public bool ExemptFromReconciliation { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime? PaidUtc { get; set; }

    public decimal? PaymentReconciliationAmount { get; set; }

    public Guid? PaymentReconciliationId { get; set; }

    public string Reference { get; set; } = string.Empty;

    public Guid? SitePaymentSettingId { get; set; }

    public decimal CalculateReconciliationAmount(decimal commission)
    {
        if (commission < 0 || commission > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(commission), "Commission must be between 0 and 1");
        }

        return Math.Round(Amount * (1 - commission), 2);
    }
}
