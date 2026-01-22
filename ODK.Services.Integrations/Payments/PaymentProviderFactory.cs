using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly ILoggingService _loggingService;
    private readonly StripePaymentProviderSettings _stripeSettings;

    public PaymentProviderFactory(
        ILoggingService loggingService,
        StripePaymentProviderSettings stripeSettings)
    {
        _loggingService = loggingService;
        _stripeSettings = stripeSettings;
    }

    public IPaymentProvider GetPaymentProvider(
        SitePaymentSettings sitePaymentSettings,
        ChapterPaymentAccount? paymentAccount)
    {
        return GetPaymentProvider(sitePaymentSettings, paymentAccount?.ExternalId);
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

    private IPaymentProvider GetPaymentProvider(SitePaymentSettings settings, string? connectedAccountId)
    {
        switch (settings.Provider)
        {
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