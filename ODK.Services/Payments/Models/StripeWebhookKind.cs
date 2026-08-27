namespace ODK.Services.Payments.Models;

/// <summary>
/// The kinds of Stripe webhook endpoint an account is expected to have, one each.
/// </summary>
/// <remarks>
/// The values are the <c>v</c> query parameter the endpoint's URL carries, which is what
/// <c>WebhooksController.Stripe</c> reads to pick a signing secret - so they are stated explicitly, and
/// renumbering one would reinterpret every endpoint already registered in a Stripe dashboard.
///
/// Not merely a label: a listed Stripe webhook endpoint carries no <c>connect</c> flag - Stripe accepts one
/// on create and never returns it - so the <c>v</c> in the URL is the only thing that says which kind an
/// endpoint is.
/// </remarks>
public enum StripeWebhookKind
{
    None = 0,

    /// <summary>Events from the platform's own Stripe account - site subscriptions.</summary>
    Site = 1,

    /// <summary>Events from the connected accounts groups take their payments through.</summary>
    ConnectedAccount = 2
}
