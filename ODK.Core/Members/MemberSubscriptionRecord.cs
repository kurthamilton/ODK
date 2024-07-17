namespace ODK.Core.Members;

public class MemberSubscriptionRecord : IDatabaseEntity
{
    public double Amount { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public int Months { get; set; }

    public DateTime PurchaseDate { get; set; }

    public SubscriptionType Type { get; set; }
}
