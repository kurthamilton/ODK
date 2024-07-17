namespace ODK.Core.Payments;

public class Payment : IDatabaseEntity
{
    public double Amount { get; set; }

    public string CurrencyCode { get; set; } = "";

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime PaidDate { get; set; }

    public string Reference { get; set; } = "";
}
