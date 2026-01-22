using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SiteEmailSettingsRepository : ReadWriteRepositoryBase<SiteEmailSettings>,
    ISiteEmailSettingsRepository
{
    public SiteEmailSettingsRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<SiteEmailSettings> Get(PlatformType platform) => Set()
        .Where(x => x.Platform == platform)
        .DeferredSingle();
}