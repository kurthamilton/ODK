namespace ODK.Core.Members;

public class MemberSiteSubscriptionRecord : IDatabaseEntity
{
    public DateTime CreatedUtc { get; init; }

    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public Guid? SiteSubscriptionPriceId { get; set; }

    public Guid SiteSubscriptionId { get; set; }
}
