using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class SitePaymentProductRepository : ReadWriteRepositoryBase<SitePaymentProduct>, ISitePaymentProductRepository
{
    public SitePaymentProductRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<SitePaymentProduct> GetByPlatform(
        PlatformType platform, EnvironmentType environment, PaymentProviderType provider)
        => Set()
            .Where(x =>
                x.Platform == platform &&
                x.Environment == environment &&
                x.PaymentProvider == provider)
            .DeferredSingleOrDefault();
}
