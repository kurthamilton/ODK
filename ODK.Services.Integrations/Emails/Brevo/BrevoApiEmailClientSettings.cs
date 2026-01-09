namespace ODK.Services.Integrations.Emails.Brevo;

public class BrevoApiEmailClientSettings
{
    public required string ApiKey { get; init; }

    public required string? DebugEmailAddress { get; init; }
}
