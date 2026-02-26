namespace ODK.Core.Members;

public class MemberSiteSubscription : IDatabaseEntity, IMemberEntity
{
    public DateTime? ExpiresUtc { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid SiteSubscriptionId { get; set; }

    public Guid? SiteSubscriptionPriceId { get; set; }

    public bool IsExpired()
    {
        // TODO: implement
        return false;
    }
}