using ODK.Core.Platforms;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripeWebhookParserSettings
{
    public required IReadOnlyDictionary<PlatformType, string> WebhookSecretsV1 { get; init; }

    public required IReadOnlyDictionary<PlatformType, string> WebhookSecretsV2 { get; init; }
}
