using ODK.Core.Subscriptions;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSubscriptionFeatureRepository : ReadWriteRepositoryBase<SiteSubscriptionFeature>, ISiteSubscriptionFeatureRepository
{
    public SiteSubscriptionFeatureRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<SiteSubscriptionFeature> GetBySiteSubscriptionId(Guid siteSubscriptionId)
        => Set()
            .Where(x => x.SiteSubscriptionId == siteSubscriptionId)
            .DeferredMultiple();
}