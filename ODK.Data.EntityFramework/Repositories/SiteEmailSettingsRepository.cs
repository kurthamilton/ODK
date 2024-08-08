using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteEmailSettingsRepository : CachingReadWriteRepositoryBase<SiteEmailSettings>,
    ISiteEmailSettingsRepository
{
    private static readonly EntityCache<Guid, SiteEmailSettings> _cache = new DatabaseEntityCache<SiteEmailSettings>();

    public SiteEmailSettingsRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingle<SiteEmailSettings> Get() => Set()
        .DeferredSingle(
            () => _cache.GetAll()?.FirstOrDefault(),
            x => _cache.SetAll([x]));
}
