namespace ODK.Services.Integrations.Payments.Stripe;

public class StripeWebhookParserSettings
{
    public required string WebhookSecretV1 { get; init; }

    public required string WebhookSecretV2 { get; init; }
}
