using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteEmailSettingsRepository : IReadWriteRepository<SiteEmailSettings>
{
    IDeferredQuerySingle<SiteEmailSettings> Get(PlatformType platform);
}
