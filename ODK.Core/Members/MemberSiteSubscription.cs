using ODK.Core.Subscriptions;

namespace ODK.Core.Members;

public class MemberSiteSubscription
{
    public DateTime? ExpiresUtc { get; set; }

    public Guid MemberId { get; set; }

    public SiteSubscription SiteSubscription { get; set; } = null!;

    public Guid SiteSubscriptionId { get; set; }
}
