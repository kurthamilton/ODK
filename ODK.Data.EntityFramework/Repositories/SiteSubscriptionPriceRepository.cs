using Microsoft.EntityFrameworkCore;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionPriceRepository : ReadWriteRepositoryBase<SiteSubscriptionPrice>, ISiteSubscriptionPriceRepository
{
    public SiteSubscriptionPriceRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscriptionPrice> GetAllEnabled()
    {
        var query = 
            from price in Set()
            from subscription in Set<SiteSubscription>().Where(x => x.Enabled)
            where price.SiteSubscriptionId == subscription.Id
            select price;
        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<SiteSubscriptionPrice> GetBySiteSubscriptionId(Guid siteSubscriptionId) => Set()
        .Where(x => x.SiteSubscriptionId == siteSubscriptionId)
        .DeferredMultiple();

    protected override IQueryable<SiteSubscriptionPrice> Set() => base.Set()
        .Include(x => x.Currency);
}
