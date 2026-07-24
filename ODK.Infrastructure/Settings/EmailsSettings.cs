namespace ODK.Infrastructure.Settings;

public class EmailsSettings
{
    public string? DebugEmailAddress { get; set; }

    public required EmailsThemeSettings Theme { get; init; }

    /// <summary>
    /// When true, emails are logged via ConsoleEmailClient instead of sent - set in appsettings.e2e.json
    /// so the E2E environment doesn't hit the real email provider.
    /// </summary>
    public bool UseConsoleClient { get; set; }
}