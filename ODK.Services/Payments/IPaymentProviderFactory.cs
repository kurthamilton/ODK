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

    /// <summary>
    /// The webhook reader for the platform's account, or null where the provider has no such thing. Kept
    /// here rather than on <see cref="IPaymentProvider"/> so a caller never has to branch on
    /// <see cref="PaymentProviderType"/> itself.
    /// </summary>
    IStripeWebhookProvider? GetStripeWebhookProvider(PaymentProviderType provider, PlatformType platform);
}