namespace ODK.Infrastructure.Settings;

public class BrevoSettings
{
    public required string ApiKey { get; init; }

    /// <summary>
    /// Prepended to <see cref="AppSettings.Environment"/> to form the environment tag. The same value in every
    /// environment - it is what marks a tag as ours, not what distinguishes one environment from another,
    /// and a receiver needs it to recognise a tag belonging to a deployment other than its own. Empty means
    /// unconfigured: sends carry no tag and received webhooks are not checked.
    /// </summary>
    public required string EnvironmentTagPrefix { get; init; }

    public required string WebhookPassword { get; init; }

    public required string WebhookPasswordHeader { get; init; }
}
