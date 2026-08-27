namespace ODK.Services.Payments.Models;

/// <summary>
/// The comparisons made against one Stripe webhook endpoint Stripe will deliver to. A disabled endpoint is
/// compared against nothing, so whether an endpoint is enabled is not among these - see
/// <see cref="StripeWebhookAuditResult.DisabledEndpoints"/>.
/// </summary>
public enum StripeWebhookCheckType
{
    None = 0,

    /// <summary>The events the endpoint is subscribed to.</summary>
    Events,

    /// <summary>The scheme and authority the endpoint's URL addresses.</summary>
    Host,

    /// <summary>Whether the endpoint belongs to a live-mode account, which its record's environment implies.</summary>
    LiveMode,

    /// <summary>The path the endpoint's URL addresses.</summary>
    Path,

    /// <summary>The platform the URL's <c>p</c> parameter names.</summary>
    Platform,

    /// <summary>Whether the URL carries a query parameter that means nothing to the app.</summary>
    Query,

    /// <summary>The <c>v</c> parameter, which is what says which kind of endpoint this is.</summary>
    Version
}
