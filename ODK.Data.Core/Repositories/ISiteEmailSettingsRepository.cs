using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISiteEmailSettingsRepository : IReadWriteRepository<SiteEmailSettings>
{
    IDeferredQuerySingle<SiteEmailSettings> Get();
}
