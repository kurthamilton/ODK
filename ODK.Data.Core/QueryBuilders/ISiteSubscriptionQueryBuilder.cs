using ODK.Core.Features;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Subscriptions;

namespace ODK.Data.Core.QueryBuilders;

public interface ISiteSubscriptionQueryBuilder : IDatabaseEntityQueryBuilder<SiteSubscription, ISiteSubscriptionQueryBuilder>
{
    /// <summary>
    /// Usable subscriptions: enabled, and either free or priced. Mirrors
    /// <see cref="SiteSubscription.IsActive"/>, which decides the same thing for a loaded subscription -
    /// the two must agree.
    /// </summary>
    ISiteSubscriptionQueryBuilder Active();

    IQueryBuilder<SiteSubscriptionFeature> Features();

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder ForPlatform(PlatformType platform);

    IQueryBuilder<SiteSubscriptionWithFeaturesDto> WithFeatures();
}