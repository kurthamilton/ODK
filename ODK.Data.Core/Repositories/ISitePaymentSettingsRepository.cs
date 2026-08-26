using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ISitePaymentSettingsRepository : IReadWriteRepository<SitePaymentSettings>
{
    IDeferredQuerySingle<SitePaymentSettings> GetActive(PlatformType platform);

    /// <summary>
    /// Every platform's settings. For resolving a <c>SitePaymentSettingId</c> already stored against a
    /// payment, subscription or connected account, which names the account that transaction was made
    /// under whatever platform it belongs to. Use <see cref="GetAll(PlatformType)"/> to list a
    /// platform's own settings.
    /// </summary>
    IDeferredQueryMultiple<SitePaymentSettings> GetAll();

    IDeferredQueryMultiple<SitePaymentSettings> GetAll(PlatformType platform);
}
