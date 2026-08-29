using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISitePaymentProductRepository : IReadWriteRepository<SitePaymentProduct>
{
    IDeferredQuerySingleOrDefault<SitePaymentProduct> GetByPlatform(
        PlatformType platform, EnvironmentType environment, PaymentProviderType provider);
}
