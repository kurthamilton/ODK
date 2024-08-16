using ODK.Core.Payments;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISitePaymentSettingsRepository : IReadWriteRepository<SitePaymentSettings>
{
    IDeferredQuerySingle<SitePaymentSettings> Get();
}
