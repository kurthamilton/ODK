using ODK.Core.Features;

namespace ODK.Core.Subscriptions;

public class SiteSubscriptionFeature : IDatabaseEntity
{
    public SiteFeatureType Feature { get; set; }

    public Guid Id { get; set; }

    public Guid SiteSubscriptionId { get; set; }
}