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

    ISiteSubscriptionQueryBuilder ForEnvironment(EnvironmentType environment);

    IQueryBuilder<SiteSubscriptionFeature> Features();

    /// <summary>Subscriptions that fall back to the named one when they lapse.</summary>
    ISiteSubscriptionQueryBuilder ForFallback(Guid siteSubscriptionId);

    IDeferredQuery<bool> HasFeature(SiteFeatureType feature);

    ISiteSubscriptionQueryBuilder ForPlatform(PlatformType platform);

    IQueryBuilder<SiteSubscriptionWithFeaturesDto> WithFeatures();
}