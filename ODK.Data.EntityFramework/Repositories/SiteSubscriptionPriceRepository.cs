using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionPriceRepository : WriteRepositoryBase<SiteSubscriptionPrice>, ISiteSubscriptionPriceRepository
{
    public SiteSubscriptionPriceRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscriptionPrice> GetBySiteSubscriptionId(Guid siteSubscriptionId) => Set()
        .Where(x => x.SiteSubscriptionId == siteSubscriptionId)
        .DeferredMultiple();
}
