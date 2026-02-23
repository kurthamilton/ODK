using ODK.Services.Payments.Models;

namespace ODK.Services.Payments;

public class ExternalCheckoutSession
{
    public required long Amount { get; init; }

    public required string ClientSecret { get; init; }

    public required DateTime? CompletedUtc { get; init; }

    public required string Currency { get; init; }

    public required IReadOnlyDictionary<string, string> Metadata { get; init; }

    public required string? PaymentId { get; init; }

    public required string SessionId { get; init; }

    public required string? SubscriptionId { get; init; }
}
