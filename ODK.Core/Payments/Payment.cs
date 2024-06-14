namespace ODK.Core.Payments;

public class Payment
{
    public Payment(Guid id, Guid memberId, DateTime paidDate, string currencyCode, double amount, string reference)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
        Id = id;
        MemberId = memberId;
        PaidDate = paidDate;
        Reference = reference;
    }

    public double Amount { get; }

    public string CurrencyCode { get; }

    public Guid Id { get; }

    public Guid MemberId { get; }

    public DateTime PaidDate { get; }

    public string Reference { get; }
}
