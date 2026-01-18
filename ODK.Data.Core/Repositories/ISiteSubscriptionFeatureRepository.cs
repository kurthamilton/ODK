using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionFeatureRepository : IReadWriteRepository<SiteSubscriptionFeature>
{
    IDeferredQueryMultiple<SiteSubscriptionFeature> GetBySiteSubscriptionId(Guid siteSubscriptionId);
}