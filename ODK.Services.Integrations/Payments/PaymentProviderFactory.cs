using ODK.Core.Payments;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments;

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
            default:
                throw new InvalidOperationException($"Payment provider type {settings.Provider} not supported");
        }
    }
}
