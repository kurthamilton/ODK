namespace ODK.Core.Members;

public class MemberSubscriptionRecord
{
    public MemberSubscriptionRecord(Guid memberId, SubscriptionType type, DateTime purchaseDate, double amount, int months)
    {
        Amount = amount;
        MemberId = memberId;
        Months = months;
        PurchaseDate = purchaseDate;
        Type = type;
    }

    public double Amount { get; }

    public Guid MemberId { get; }

    public int Months { get; }

    public DateTime PurchaseDate { get; }

    public SubscriptionType Type { get; }
}
