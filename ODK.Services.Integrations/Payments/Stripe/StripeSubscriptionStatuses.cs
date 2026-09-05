namespace ODK.Services.Integrations.Payments.Stripe;

internal static class StripeSubscriptionStatuses
{
    internal const string Active = "active";

    /// <summary>Not a status a subscription has - what a list request asks for to be given every one.</summary>
    internal const string All = "all";

    internal const string Canceled = "canceled";

    internal const string Incomplete = "incomplete";

    internal const string IncompleteExpired = "incomplete_expired";

    internal const string PastDue = "past_due";

    internal const string Paused = "paused";

    internal const string Trialing = "trialing";

    internal const string Unpaid = "unpaid";
}
