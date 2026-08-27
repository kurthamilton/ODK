using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Platforms;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private readonly StripePaymentProviderSettings _stripeSettings;

    public PaymentProviderFactory(
        ILoggingService loggingService,
        StripePaymentProviderSettings stripeSettings,
        IPlatformProvider platformProvider)
    {
        _loggingService = loggingService;
        _platformProvider = platformProvider;
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

    /* Guarded on the provider type rather than constructing and testing the result, because
       GetPaymentProvider throws for a type it does not support - and a record on another provider is a
       caller asking a fair question, not a fault. */
    public IStripeWebhookProvider? GetStripeWebhookProvider(SitePaymentSettings sitePaymentSettings)
        => sitePaymentSettings.Provider == PaymentProviderType.Stripe
            ? GetPaymentProvider(sitePaymentSettings, connectedAccountId: null) as IStripeWebhookProvider
            : null;

    private IPaymentProvider GetPaymentProvider(SitePaymentSettings settings, string? connectedAccountId)
    {
        switch (settings.Provider)
        {
            case PaymentProviderType.Stripe:
                return new StripePaymentProvider(
                    settings,
                    _loggingService,
                    connectedAccountId: connectedAccountId,
                    _stripeSettings,
                    _platformProvider);

            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}