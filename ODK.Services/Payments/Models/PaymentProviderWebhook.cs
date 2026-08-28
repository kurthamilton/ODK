using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

public class PaymentProviderWebhook
{
    public required decimal Amount { get; init; }

    public required bool Complete { get; init; }

    public required string Id { get; init; }

    /// <summary>
    /// The invoice the event describes, where it describes one. A recurring subscription's payment names
    /// only its invoice, so this is the handle on what it charged.
    /// </summary>
    public required string? InvoiceId { get; init; }

    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    public required DateTime OriginatedUtc { get; init; }

    public required string? PaymentId { get; init; }

    public required PaymentProviderType PaymentProviderType { get; init; }

    public required string? SubscriptionId { get; init; }

    public required PaymentProviderWebhookType? Type { get; init; }
}
