using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SitePaymentSettingsRepository : CachingReadWriteRepositoryBase<SitePaymentSettings>, ISitePaymentSettingsRepository
{
    private static readonly EntityCache<Guid, SitePaymentSettings> _cache = new DatabaseEntityCache<SitePaymentSettings>();

    public SitePaymentSettingsRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingle<SitePaymentSettings> Get() => Set()
        .DeferredSingle(
            () => _cache.GetAll()?.FirstOrDefault(),
            x => _cache.SetAll([x]));
}
