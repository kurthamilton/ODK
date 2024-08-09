using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionRepository : ReadWriteRepositoryBase<SiteSubscription>, ISiteSubscriptionRepository
{
    public SiteSubscriptionRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscription> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQueryMultiple<SiteSubscription> GetAllEnabled() => Set()
        .Where(x => x.Enabled)
        .DeferredMultiple();

    public IDeferredQuerySingle<SiteSubscription> GetDefault() => Set()
        .Where(x => x.Enabled && x.Default)
        .DeferredSingle();
}
