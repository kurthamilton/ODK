namespace ODK.Services.Integrations.Emails.Smtp;

public class SmtpEmailClientSettings
{
    public required string Host { get; init; }

    public required int Port { get; init; }
}
