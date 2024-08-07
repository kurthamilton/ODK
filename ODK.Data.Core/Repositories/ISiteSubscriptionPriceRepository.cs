using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionPriceRepository : IWriteRepository<SiteSubscriptionPrice>
{
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetBySiteSubscriptionId(Guid siteSubscriptionId);
}
