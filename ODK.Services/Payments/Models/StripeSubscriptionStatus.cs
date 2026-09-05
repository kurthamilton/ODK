namespace ODK.Services.Payments.Models;

/// <summary>
/// A Stripe subscription's status, in Stripe's own terms rather than reduced to active or not.
/// </summary>
/// <remarks>
/// Fuller than <see cref="ExternalSubscriptionStatus"/>, which answers "is this still running" for the
/// checkout paths. Here the distinction that matters is whether the subscription will bill again, since that
/// is what separates metadata that is already costing us from metadata that will.
/// </remarks>
public enum StripeSubscriptionStatus
{
    None = 0,
    Active,
    Cancelled,
    Incomplete,
    IncompleteExpired,
    Paused,
    PastDue,
    Trialing,
    Unpaid
}
