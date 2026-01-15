namespace ODK.Web.Common.Config.Settings;

public class BrevoSettings
{
    public required string ApiKey { get; init; }

    public required string WebhookEnv { get; init; }

    public required string WebhookEnvHeader { get; init; }

    public required string WebhookPassword { get; init; }

    public required string WebhookPasswordHeader { get; init; }
}