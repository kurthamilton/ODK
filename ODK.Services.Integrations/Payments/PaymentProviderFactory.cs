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
    private readonly StripePaymentProviderSettings _stripeSettings;

    public PaymentProviderFactory(
        IHttpClientFactory httpClientFactory,
        PayPalPaymentProviderSettings payPalSettings,
        ILoggingService loggingService,
        StripePaymentProviderSettings stripeSettings)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _payPalSettings = payPalSettings;
        _stripeSettings = stripeSettings;
    }

    public IPaymentProvider GetPaymentProvider(
        ChapterPaymentSettings? chapterPaymentSettings,
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        ChapterPaymentAccount? paymentAccount)
    {
        IPaymentSettings paymentSettings = chapterPaymentSettings == null || chapterPaymentSettings.UseSitePaymentProvider == true
            ? sitePaymentSettings.First(x => x.Active)
            : chapterPaymentSettings;

        return GetPaymentProvider(paymentSettings, paymentAccount?.ExternalId);
    }

    public IPaymentProvider GetSitePaymentProvider(SitePaymentSettings settings)
    {
        return GetPaymentProvider(settings, connectedAccountId: null);
    }

    public IPaymentProvider GetSitePaymentProvider(
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        Guid? sitePaymentSettingId)
    {
        var paymentSettings = sitePaymentSettingId != null
            ? sitePaymentSettings.First(x => x.Id == sitePaymentSettingId.Value)
            : sitePaymentSettings.First(x => x.Active);

        return GetPaymentProvider(paymentSettings, connectedAccountId: null);
    }

    private IPaymentProvider GetPaymentProvider(IPaymentSettings settings, string? connectedAccountId)
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
                    connectedAccountId: connectedAccountId,
                    _stripeSettings);
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}
