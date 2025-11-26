using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments.Models;

public class PaymentProviderWebhook
{
    public required bool Complete { get; init; }

    public required string Id { get; init; }

    public required IDictionary<string, string> Metadata { get; init; }

    public required string? PaymentId { get; init; }

    public required PaymentProviderType PaymentProviderType { get; init; }

    public required PlatformType Platform { get; init; }

    public required string? SubscriptionId { get; init; }

    public required PaymentProviderWebhookType? Type { get; init; }
}
