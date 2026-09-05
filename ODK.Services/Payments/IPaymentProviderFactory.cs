using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments;

public interface IPaymentProviderFactory
{
    IPaymentProvider GetPaymentProvider(PlatformType platform);

    IPaymentProvider GetPaymentProvider(
        PaymentProviderType provider, PlatformType platform);

    /// <summary>
    /// The provider for the type, or null where none is implemented for it. For a caller sweeping records
    /// written under several providers, where one we cannot talk to is to be passed over rather than a fault.
    /// </summary>
    IPaymentProvider? GetPaymentProviderOrDefault(PaymentProviderType provider, PlatformType platform);

    IStripeTransactionProvider? GetStripeTransactionProvider(PlatformType platform);

    IStripeWebhookProvider? GetStripeWebhookProvider(PlatformType platform);
}