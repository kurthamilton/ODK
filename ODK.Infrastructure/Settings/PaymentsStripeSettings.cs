namespace ODK.Infrastructure.Settings;

public class PaymentsStripeSettings
{
    public required string ConnectedAccountBaseUrl { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }

    public required string WebhookSecretV1 { get; init; }

    public required string WebhookSecretV2 { get; init; }
}
