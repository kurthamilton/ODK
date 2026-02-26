using ODK.Core.Subscriptions;

namespace ODK.Data.Core.Subscriptions;

public class SiteSubscriptionSummaryDto
{
    public required int ActiveMemberSiteSubscriptionCount { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }
}