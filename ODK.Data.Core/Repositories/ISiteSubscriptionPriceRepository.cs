using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionPriceRepository : IReadWriteRepository<SiteSubscriptionPrice>
{
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetAllEnabled();
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetBySiteSubscriptionId(Guid siteSubscriptionId);
}
