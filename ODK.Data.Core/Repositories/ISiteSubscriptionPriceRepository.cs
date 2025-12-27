using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSubscriptionPriceRepository : IReadWriteRepository<SiteSubscriptionPrice>
{
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetAll(PlatformType platform);
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetAllEnabled(PlatformType platform);
    IDeferredQueryMultiple<SiteSubscriptionPrice> GetBySiteSubscriptionId(Guid siteSubscriptionId);
}
