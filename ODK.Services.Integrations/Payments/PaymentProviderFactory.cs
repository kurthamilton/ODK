using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Platforms;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private readonly PaymentProviderFactorySettings _settings;
    private readonly StripePaymentProviderSettings _stripeSettings;

    public PaymentProviderFactory(
        ILoggingService loggingService,
        StripePaymentProviderSettings stripeSettings,
        IPlatformProvider platformProvider,
        PaymentProviderFactorySettings settings)
    {
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _settings = settings;
        _stripeSettings = stripeSettings;
    }

    public IPaymentProvider GetPaymentProvider(PlatformType platform)
        => GetPaymentProvider(_settings.DefaultProvider, platform);

    public IPaymentProvider GetPaymentProvider(PaymentProviderType provider, PlatformType platform)
    {
        switch (provider)
        {
            case PaymentProviderType.Stripe:
                return new StripePaymentProvider(
                    _loggingService,
                    _stripeSettings,
                    _platformProvider,
                    platform);

            default:
                throw new InvalidOperationException($"Payment provider type {provider} not supported");
        }
    }

    /* Guarded on the provider type rather than constructing and testing the result, because
       GetPaymentProvider throws for a type it does not support - and a record on another provider is a
       caller asking a fair question, not a fault. */
    public IPaymentProvider? GetPaymentProviderOrDefault(PaymentProviderType provider, PlatformType platform)
        => provider == PaymentProviderType.Stripe
            ? GetPaymentProvider(provider, platform)
            : null;

    /// <inheritdoc cref="GetPaymentProviderOrDefault"/>
    public IStripeTransactionProvider? GetStripeTransactionProvider(PlatformType platform)
        => GetPaymentProvider(PaymentProviderType.Stripe, platform) as IStripeTransactionProvider;

    /// <inheritdoc cref="GetPaymentProviderOrDefault"/>
    public IStripeWebhookProvider? GetStripeWebhookProvider(PlatformType platform)
        => GetPaymentProvider(PaymentProviderType.Stripe, platform) as IStripeWebhookProvider;
}