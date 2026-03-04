using ODK.Core.Subscriptions;

namespace ODK.Data.Core.Subscriptions;

public class SiteSubscriptionWithFeaturesDto
{
    public required IReadOnlyCollection<SiteSubscriptionFeature> Features { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }
}