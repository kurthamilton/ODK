using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly PayPalPaymentProviderSettings _payPalSettings;

    public PaymentProviderFactory(
        IHttpClientFactory httpClientFactory,
        PayPalPaymentProviderSettings payPalSettings,
        ILoggingService loggingService)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _payPalSettings = payPalSettings;
    }

    public IPaymentProvider GetChapterPaymentProvider(
        ChapterPaymentSettings chapterPaymentSettings,
        IChapterPaymentEntity paymentEntity)
    {
        IPaymentSettings paymentSettings = paymentEntity.SitePaymentSettings != null
            ? paymentEntity.SitePaymentSettings
            : chapterPaymentSettings;
        return GetPaymentProvider(paymentSettings, paymentEntity.ExternalAccountId);
    }

    public IPaymentProvider GetChapterPaymentProvider(
        SitePaymentSettings sitePaymentSettings,
        ChapterPaymentSettings chapterPaymentSettings,
        ChapterPaymentAccount? chapterPaymentAccount)
    {
        IPaymentSettings paymentSettings = chapterPaymentSettings.UseSitePaymentProvider
            ? sitePaymentSettings
            : chapterPaymentSettings;

        return GetPaymentProvider(paymentSettings, chapterPaymentAccount?.ExternalId);
    }

    public IPaymentProvider GetPaymentProvider(IPaymentSettings settings, string? connectedAccountId)
    {
        switch (settings.Provider)
        {
            case PaymentProviderType.PayPal:
                return new PayPalPaymentProvider(
                    _payPalSettings,
                    _httpClientFactory,
                    settings
                    );
            case PaymentProviderType.Stripe:
                return new StripePaymentProvider(
                    settings,
                    _loggingService,
                    connectedAccountId);
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }

    public IPaymentProvider GetSitePaymentProvider(SitePaymentSettings sitePaymentSettings)
    {
        return GetPaymentProvider(sitePaymentSettings, connectedAccountId: null);
    }
}
