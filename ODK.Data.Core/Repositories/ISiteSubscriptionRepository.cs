using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Subscriptions;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionRepository : IReadWriteRepository<SiteSubscription, ISiteSubscriptionQueryBuilder>
{
    IDeferredQueryMultiple<SiteSubscription> GetAll(PlatformType platform);

    IDeferredQuerySingle<SiteSubscription> GetByPriceId(Guid priceId);

    IDeferredQuerySingleOrDefault<SiteSubscription> GetByPriceIdOrDefault(Guid priceId);

    IDeferredQuerySingle<SiteSubscription> GetDefault(EnvironmentType environment, PlatformType platform);

    /// <summary>
    /// The platform's default plan, or null where it has none enabled - for a caller that has to decide
    /// what to do about the absence rather than fail on it.
    /// </summary>
    IDeferredQuerySingleOrDefault<SiteSubscription> GetDefaultOrDefault(
        EnvironmentType environment, PlatformType platform);

    IDeferredQueryMultiple<SiteSubscriptionSummaryDto> GetSummaries(
        PlatformType platform, SiteSubscriptionCooldown cooldown);
}