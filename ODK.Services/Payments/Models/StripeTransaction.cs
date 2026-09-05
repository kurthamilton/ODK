namespace ODK.Services.Payments.Models;

/// <summary>
/// One movement of money as Stripe reports it, with the metadata that would have identified it to us.
/// </summary>
/// <remarks>
/// Assembled from an invoice where a subscription billed it and from a payment intent where nothing did,
/// because in this API version neither a charge nor a payment intent names the invoice that raised it - see
/// <c>IStripeTransactionProvider.ListTransactions</c>. So which ids are present says how the money was
/// taken, and any of them can be absent.
/// </remarks>
public class StripeTransaction
{
    /// <summary>What was paid. Zero on a transaction that has not been paid.</summary>
    public required decimal Amount { get; init; }

    /// <summary>The charge the money arrived on, where the provider has said which one.</summary>
    public required string? ChargeId { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required string CurrencyCode { get; init; }

    /// <summary>The invoice that billed it. Null for a one-off, which is billed by nothing.</summary>
    public required string? InvoiceId { get; init; }

    public required StripeTransactionKind Kind { get; init; }

    /// <summary>
    /// The metadata a webhook for this would carry, read through the same path the webhook reads it: the
    /// subscription details on the invoice where a subscription billed it, and the payment intent's own
    /// metadata where nothing did. So this is what the app would have seen, not what the objects behind it
    /// say now - compare <see cref="StripeSubscription.Metadata"/>, which is what a subscription says now.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    /// <summary>
    /// When Stripe recorded the money as taken, where it says - which is not
    /// <see cref="CreatedUtc"/> on an invoice retried after a failed card. Null where nothing on the object
    /// states it.
    /// </summary>
    public required DateTime? PaidUtc { get; init; }

    public required string? PaymentIntentId { get; init; }

    public required StripeTransactionStatus Status { get; init; }

    /// <summary>The subscription that billed it, where one did.</summary>
    public required string? SubscriptionId { get; init; }

    /// <summary>
    /// What to address the transaction by: the payment intent, else the invoice, else the charge. Every
    /// transaction has at least one of the three, so this is never empty.
    /// </summary>
    public string Reference => PaymentIntentId ?? InvoiceId ?? ChargeId ?? string.Empty;
}
