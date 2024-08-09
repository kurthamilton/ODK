using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionRepository : IReadWriteRepository<SiteSubscription>
{
    IDeferredQueryMultiple<SiteSubscription> GetAll(PlatformType platform);
    IDeferredQueryMultiple<SiteSubscription> GetAllEnabled(PlatformType platform);
    IDeferredQuerySingle<SiteSubscription> GetDefault(PlatformType platform);
}
