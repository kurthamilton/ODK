namespace ODK.Services.Integrations.Payments.Stripe;

/// <summary>
/// Why Stripe issued an invoice. Only the one that says an invoice created a subscription is named: every
/// other reason an invoice exists on a subscription is a later billing of it, and treating an unrecognised
/// reason as a renewal is the safe way round - a renewal is audited, a first invoice is the one already
/// known to have come from checkout.
/// </summary>
internal static class StripeInvoiceBillingReasons
{
    internal const string SubscriptionCreate = "subscription_create";
}
