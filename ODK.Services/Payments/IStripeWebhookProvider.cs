using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

/// <summary>
/// Reads the webhook endpoints registered against one Stripe account.
/// </summary>
/// <remarks>
/// Deliberately outside <see cref="IPaymentProvider"/>: what a webhook is differs enough between providers
/// that a shared abstraction would be shape imposed for its own sake. A caller wanting this asks the factory
/// for it and gets nothing back where the provider does not offer it.
/// </remarks>
public interface IStripeWebhookProvider
{
    Task<IReadOnlyCollection<StripeWebhookEndpoint>> ListWebhooks();
}
