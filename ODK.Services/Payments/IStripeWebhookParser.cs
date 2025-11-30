using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public interface IStripeWebhookParser
{
    Task<PaymentProviderWebhook?> ParseWebhook(string json, string? signature);
}
