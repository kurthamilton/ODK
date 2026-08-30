namespace ODK.Infrastructure.Settings;

public class StripePlatformSettings
{
    /// <summary>
    /// The provider's own id for the account this platform transacts as. Not a secret - it names the
    /// account in the provider's dashboard - and what a webhook endpoint's address is built from.
    /// </summary>
    public required string AccountId { get; init; }

    public required string ConnectedAccountBaseUrl { get; init; }

    public required string PublicApiKey { get; init; }

    public required string SecretApiKey { get; init; }

    public required string WebhookSecretV1 { get; init; }

    public required string WebhookSecretV2 { get; init; }
}
