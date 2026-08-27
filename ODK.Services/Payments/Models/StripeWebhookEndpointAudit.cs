namespace ODK.Services.Payments.Models;

/// <summary>
/// One Stripe webhook endpoint, with what the audit found about it.
/// </summary>
public class StripeWebhookEndpointAudit
{
    public required IReadOnlyCollection<StripeWebhookCheck> Checks { get; init; }

    public required StripeWebhookEndpoint Endpoint { get; init; }

    /// <summary>
    /// Events the endpoint receives that nothing expects. Informational: an event the app ignores costs
    /// nothing, where a missing one loses a payment.
    /// </summary>
    public required IReadOnlyCollection<string> ExtraEvents { get; init; }

    /// <summary>
    /// Which of the account's two endpoints this is, read from the URL's <c>v</c> parameter.
    /// <see cref="StripeWebhookKind.None"/> where that parameter is missing or says nothing the app knows,
    /// which is an endpoint nothing will route.
    /// </summary>
    public required StripeWebhookKind Kind { get; init; }

    /// <summary>Expected events the endpoint is not subscribed to. Empty where nothing is expected.</summary>
    public required IReadOnlyCollection<string> MissingEvents { get; init; }
}
