namespace ODK.Services.Payments.Models;

/// <summary>
/// A Stripe subscription as Stripe reports it now, which is what its next invoice will carry.
/// </summary>
public class StripeSubscription
{
    public required DateTime CreatedUtc { get; init; }

    public required string? CustomerId { get; init; }

    public required string Id { get; init; }

    /// <summary>
    /// What Stripe will copy onto the next invoice it issues, and therefore what the next renewal webhook
    /// will carry. Correcting a subscription in the dashboard changes this and nothing already invoiced.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    public required StripeSubscriptionStatus Status { get; init; }

    /// <summary>
    /// Whether Stripe will bill the subscription again, which is what separates metadata that is costing us
    /// renewals from metadata that only ever would have. A paused subscription counts: it is resumable, and
    /// nothing about it says the metadata will be looked at before it is.
    /// </summary>
    /// <remarks>
    /// Stated as the two statuses that end a subscription rather than as the several that continue it, so a
    /// status this app does not recognise reads as one that will bill - reporting a renewal at risk that is
    /// not costs a look, and missing one costs the renewal.
    /// </remarks>
    public bool BillsAgain => Status
        is not (StripeSubscriptionStatus.Cancelled or StripeSubscriptionStatus.IncompleteExpired);
}
