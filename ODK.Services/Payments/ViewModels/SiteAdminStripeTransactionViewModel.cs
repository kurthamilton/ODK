using ODK.Services.Payments.Models;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One movement of money as the overview shows it, with the payment of ours it answers.
/// </summary>
public class SiteAdminStripeTransactionViewModel
{
    public required string AmountDisplay { get; init; }

    public required string? ChargeId { get; init; }

    public required DateTime CreatedUtc { get; init; }

    /// <inheritdoc cref="SiteAdminStripeWebhookViewModel.DashboardUrl"/>
    public required string? DashboardUrl { get; init; }

    public required IReadOnlyCollection<StripeTransactionFinding> Findings { get; init; }

    public required string? InvoiceId { get; init; }

    public required StripeTransactionKind Kind { get; init; }

    public required string? MemberName { get; init; }

    /// <summary>What the transaction carries, parsed. Rendered so a broken row can be read at a glance.</summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    /// <summary>Our own payment, where one was matched.</summary>
    public required Guid? PaymentId { get; init; }

    public required string? PaymentIntentId { get; init; }

    /// <summary>What we recorded the payment as being for. Null where no payment was matched.</summary>
    public required string? Reference { get; init; }

    public required StripeTransactionStatus Status { get; init; }

    public required string? SubscriptionId { get; init; }

    public bool HasFindings => Findings.Count > 0;
}
