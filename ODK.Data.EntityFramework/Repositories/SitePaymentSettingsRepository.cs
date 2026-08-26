using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Core.Platforms;
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

    public IDeferredQuerySingle<SitePaymentSettings> GetActive(PlatformType platform)
        => Set()
            .Where(x => x.Active && x.Platform == platform)
            .DeferredSingle();

    public IDeferredQueryMultiple<SitePaymentSettings> GetAll()
        => Set()
           .DeferredMultiple();

    public IDeferredQueryMultiple<SitePaymentSettings> GetAll(PlatformType platform)
        => Set()
            .Where(x => x.Platform == platform)
            .DeferredMultiple();
}