using ODK.Core.Payments;
using ODK.Core.Web;
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

    public IPaymentProvider GetPaymentProvider(IPaymentSettings settings)
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
                    _loggingService);
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}
