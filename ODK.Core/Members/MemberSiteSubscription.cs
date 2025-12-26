using ODK.Core.Features;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Core.Members;

public class MemberSiteSubscription : IDatabaseEntity, IMemberEntity
{
    public DateTime? ExpiresUtc { get; set; }

    public string? ExternalId { get; set; }    

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public SiteSubscription SiteSubscription { get; set; } = null!;

    public Guid SiteSubscriptionId { get; set; }

    public SiteSubscriptionPrice? SiteSubscriptionPrice { get; set; }

    public Guid? SiteSubscriptionPriceId { get; set; }

    public IEnumerable<SiteFeatureType> Features() => !IsExpired() 
        ? SiteSubscription.Features() 
        : [];

    public bool HasFeature(SiteFeatureType feature) => !IsExpired() && SiteSubscription.HasFeature(feature);

    public bool IsExpired()
    {
        // TODO: implement
        return false;
    }
}
