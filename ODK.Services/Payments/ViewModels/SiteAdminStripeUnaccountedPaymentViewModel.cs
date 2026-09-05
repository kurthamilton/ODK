namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// A payment of ours recorded as paid through Stripe that nothing in the account answers.
/// </summary>
public class SiteAdminStripeUnaccountedPaymentViewModel
{
    public required string AmountDisplay { get; init; }

    /// <summary>What the payment names at Stripe - a payment intent, or the subscription that billed it.</summary>
    public required string? ExternalId { get; init; }

    public required Guid Id { get; init; }

    public required string? MemberName { get; init; }

    public required DateTime? PaidUtc { get; init; }

    public required string Reference { get; init; }
}
