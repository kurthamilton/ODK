using ODK.Core.Settings;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteSettingsRepository : IWriteRepository<SiteSettings>
{
    IDeferredQuerySingle<SiteSettings> Get();
}
