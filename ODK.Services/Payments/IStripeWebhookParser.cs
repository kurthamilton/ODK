using ODK.Core.Platforms;
using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IStripeWebhookParser
{
    Task<PaymentProviderWebhook?> ParseWebhook(PlatformType platform, string json, string? signature, int version);
}
