using ODK.Core.Settings;
using ODK.Data.Core.Deferred;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteSettingsRepository : WriteRepositoryBase<SiteSettings>, ISiteSettingsRepository
{
    public SiteSettingsRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<SiteSettings> Get() => Set()
        .DeferredSingle();
}
