using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Core.Settings;

public interface ISiteSettingsRepository : IWriteRepository<SiteSettings>
{
    IDeferredQuerySingle<SiteSettings> Get();
}
