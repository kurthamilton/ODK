using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionRepository : IReadWriteRepository<SiteSubscription>
{
    IDeferredQueryMultiple<SiteSubscription> GetAll();
    IDeferredQueryMultiple<SiteSubscription> GetAllEnabled();
    IDeferredQuerySingle<SiteSubscription> GetDefault();
}
