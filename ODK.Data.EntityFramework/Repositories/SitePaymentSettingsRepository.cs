using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SitePaymentSettingsRepository : ReadWriteRepositoryBase<SitePaymentSettings>, ISitePaymentSettingsRepository
{
    public SitePaymentSettingsRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<SitePaymentSettings> GetActive()
        => Set()
            .Where(x => x.Active)
            .DeferredSingle();

    public IDeferredQueryMultiple<SitePaymentSettings> GetAll()
        => Set()
           .DeferredMultiple();
}