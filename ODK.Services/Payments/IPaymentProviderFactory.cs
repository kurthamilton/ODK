using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public interface IPaymentProviderFactory
{
    IPaymentProvider GetChapterPaymentProvider(
        ChapterPaymentSettings chapterPaymentSettings,
        IChapterPaymentEntity paymentEntity);

    IPaymentProvider GetChapterPaymentProvider(
        SitePaymentSettings sitePaymentSettings,
        ChapterPaymentSettings chapterPaymentSettings,
        ChapterPaymentAccount? chapterPaymentAccount);

    IPaymentProvider GetPaymentProvider(IPaymentSettings settings, string? connectedAccountId);

    IPaymentProvider GetSitePaymentProvider(SitePaymentSettings sitePaymentSettings);
}
