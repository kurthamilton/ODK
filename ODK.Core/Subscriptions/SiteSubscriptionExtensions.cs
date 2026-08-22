namespace ODK.Core.Subscriptions;

public static class SiteSubscriptionExtensions
{
    /// <summary>
    /// The number of groups the subscription permits, or null when it permits any number.
    /// </summary>
    public static int? GroupLimitOrDefault(this SiteSubscription? siteSubscription)
    {
        // Do not collapse the two cases into a single null-coalesce: both a member with no
        // subscription and a subscription with no limit read as null, but the first means the
        // default limit applies and the second means no limit applies.
        return siteSubscription != null
            ? siteSubscription.GroupLimit
            : SiteSubscription.DefaultGroupLimit;
    }
}
