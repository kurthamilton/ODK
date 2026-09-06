namespace ODK.Infrastructure.Settings;

public class PrivacyLoggingSettings
{
    /// <summary>
    /// How long Better Stack keeps the logs shipped to it. Set on the Better Stack account rather than by
    /// this app, so this states what that account is configured to do and nothing here enforces it.
    /// </summary>
    public required int BetterStackRetentionDays { get; init; }

    /// <summary>
    /// How long the app's own logs are kept - the log files, and the rows the Serilog sink writes to the
    /// Logs table. Enforced by Serilog's file retention and by the log purge scheduled task.
    /// </summary>
    public required int DefaultRetentionDays { get; init; }
}
