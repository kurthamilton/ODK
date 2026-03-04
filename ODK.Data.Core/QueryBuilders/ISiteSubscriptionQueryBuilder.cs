using ODK.Core.Features;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Subscriptions;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteSubscriptionQueryBuilder : IDatabaseEntityQueryBuilder<SiteSubscription, ISiteSubscriptionQueryBuilder>
{
    ISiteSubscriptionQueryBuilder Enabled();

    IQueryBuilder<SiteSubscriptionFeature> Features();

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder ForPlatform(PlatformType platform);

    IQueryBuilder<SiteSubscriptionWithFeaturesDto> WithFeatures();
}