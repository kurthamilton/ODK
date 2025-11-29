namespace ODK.Services.Integrations.Payments.Stripe;

public class StripeWebhookParserSettings
{
    public required string WebhookSecret { get; init; }
}
