using ODK.Core.Subscriptions;

namespace ODK.Data.Core.Subscriptions;

public class SiteSubscriptionSummaryDto
{
    public required int ActiveMemberSiteSubscriptionCount { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionFeature> Features { get; init; }

    /// <summary>
    /// Every member record ever written against the subscription, expired ones included - unlike
    /// <see cref="ActiveMemberSiteSubscriptionCount"/>, which counts only those still in force. A
    /// subscription with any record at all is part of a member's payment history.
    /// </summary>
    public required int MemberSiteSubscriptionCount { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }
}