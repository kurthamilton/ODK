namespace ODK.Infrastructure.Settings;

public class EmailsSmtpSettings
{
    public required string Host { get; init; }

    public required int Port { get; init; }
}
