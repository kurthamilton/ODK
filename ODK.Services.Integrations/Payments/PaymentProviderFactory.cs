using ODK.Core.Payments;
using ODK.Core.Web;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly PayPalPaymentProviderSettings _payPalSettings;

    public PaymentProviderFactory(
        IHttpClientFactory httpClientFactory,
        PayPalPaymentProviderSettings payPalSettings,
        IHttpRequestProvider httpRequestProvider)
    {
        _httpClientFactory = httpClientFactory;
        _httpRequestProvider = httpRequestProvider;
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
                    _httpRequestProvider);
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}
