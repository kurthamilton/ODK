namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProviderPlatformSettings
{
    public required string ConnectedAccountBaseUrl { get; init; }

    public required string PublicApiKey { get; init; }

    public required string SecretApiKey { get; init; }
}
