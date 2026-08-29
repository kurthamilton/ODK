using ODK.Core.Platforms;

namespace ODK.Services.Integrations.Emails.Brevo;

public class BrevoWebhookParserSettings
{
    public required EnvironmentType Environment { get; init; }

    public required string EnvironmentTagPrefix { get; init; }
}
