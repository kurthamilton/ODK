using ODK.Core.Members;
using ODK.Core.Subscriptions;

namespace ODK.Data.Core.Members;

public class MemberSiteSubscriptionDto
{
    public required MemberSiteSubscriptionState MemberSiteSubscription { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }

    public required SiteSubscriptionPrice? SiteSubscriptionPrice { get; init; }
}