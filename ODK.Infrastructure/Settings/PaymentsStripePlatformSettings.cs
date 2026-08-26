namespace ODK.Infrastructure.Settings;

public class PaymentsStripePlatformSettings
{
    public required string ConnectedAccountBaseUrl { get; init; }

    public required string WebhookSecretV1 { get; init; }

    public required string WebhookSecretV2 { get; init; }
}
