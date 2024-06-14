using ODK.Core.Payments;
using ODK.Services.Payments.PayPal;
using ODK.Services.Payments.Stripe;

namespace ODK.Services.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IPayPalPaymentProvider _payPalPaymentProvider;
    private readonly IStripePaymentProvider _stripePaymentProvider;

    public PaymentProviderFactory(IStripePaymentProvider stripePaymentProvider,
        IPayPalPaymentProvider payPalPaymentProvider)
    {
        _payPalPaymentProvider = payPalPaymentProvider;
        _stripePaymentProvider = stripePaymentProvider;
    }

    public IPaymentProvider GetPaymentProvider(PaymentProviderType type)
    {
        switch (type)
        {
            case PaymentProviderType.PayPal:
                return _payPalPaymentProvider;
            case PaymentProviderType.Stripe:
                return _stripePaymentProvider;
            default:
                throw new ArgumentException($"Payment provider type {type} not supported", nameof(type));
        }
    }
}
