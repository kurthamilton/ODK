namespace ODK.Web.Common.Config.Settings;

public class PaymentsStripeSettings
{
    public required string WebhookSecretV1 { get; init; }

    public required string WebhookSecretV2 { get; init; }
}
