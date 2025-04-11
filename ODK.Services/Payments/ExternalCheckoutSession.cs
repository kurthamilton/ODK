namespace ODK.Services.Payments;

public class ExternalCheckoutSession
{
    public required long Amount { get; init; }

    public required string ClientSecret { get; init; }

    public required bool Complete { get; init; }

    public required string Currency { get; init; }

    public required string SessionId { get; init; }

    public required string? SubscriptionId { get; init; }
}
