using ODK.Core.Emails;
using ODK.Core.Platforms;
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

    public IDeferredQuerySingle<SiteEmailSettings> Get(PlatformType platform) => Set()
        .DeferredSingle(
            () => _cache.GetAll()?.FirstOrDefault(x => x.Platform == platform),
            _cache.Set,
            _cache.SetAll);
}
