using ODK.Core.Settings;
using ODK.Data.Core.Deferred;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSettingsRepository : CachingWriteRepositoryBase<SiteSettings, Guid>, ISiteSettingsRepository
{
    private static readonly EntityCache<Guid, SiteSettings> _cache = new EntityCache<Guid, SiteSettings>(x => x.Id);

    public SiteSettingsRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingle<SiteSettings> Get() => Set()
        .DeferredSingle(
            () => _cache.GetAll()?.FirstOrDefault(),
            x => _cache.SetAll([x]));
}
