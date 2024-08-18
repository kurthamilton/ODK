using ODK.Core.Payments;
using ODK.Services.Payments.PayPal;

namespace ODK.Services.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalPaymentProviderSettings _payPalSettings;

    public PaymentProviderFactory(
        IHttpClientFactory httpClientFactory,
        PayPalPaymentProviderSettings payPalSettings)
    {
        _httpClientFactory = httpClientFactory;
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
            //case PaymentProviderType.Stripe:
            //    return new StripePaymentProvider();
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}
