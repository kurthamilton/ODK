namespace ODK.Web.Common.Settings;

/// <summary>
/// What <c>WebhooksController</c> needs from <c>Brevo</c> configuration, mapped in <c>DependencyRegistrar</c>.
/// Named for the provider, because only Brevo's webhook authenticates on header values - Stripe's is verified by
/// signature through <c>IStripeWebhookParser</c>.
/// </summary>
public class WebhooksControllerSettings
{
    public required string BrevoWebhookPassword { get; init; }

    public required string BrevoWebhookPasswordHeader { get; init; }
}
