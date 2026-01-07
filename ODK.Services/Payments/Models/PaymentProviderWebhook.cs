using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

public class PaymentProviderWebhook
{
    public required decimal Amount { get; init; }

    public required bool Complete { get; init; }

    public required string Id { get; init; }

    public required IDictionary<string, string> Metadata { get; init; }

    public required DateTime OriginatedUtc { get; init; }

    public required string? PaymentId { get; init; }

    public required PaymentProviderType PaymentProviderType { get; init; }

    public required string? SubscriptionId { get; init; }

    public required PaymentProviderWebhookType? Type { get; init; }
}
