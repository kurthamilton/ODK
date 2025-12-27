using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentProviderFactory
{
    IPaymentProvider GetPaymentProvider(
        ChapterPaymentSettings chapterPaymentSettings,
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        ChapterPaymentAccount? paymentAccount);

    IPaymentProvider GetSitePaymentProvider(SitePaymentSettings sitePaymentSettings);

    IPaymentProvider GetSitePaymentProvider(
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        Guid? sitePaymentSettingId);
}
