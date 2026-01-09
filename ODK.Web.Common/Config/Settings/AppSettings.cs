namespace ODK.Web.Common.Config.Settings;

public class AppSettings
{
    public required AuthSettings Auth { get; init; }

    public required BetterStackSettings BetterStack { get; init; }

    public required BrevoSettings Brevo { get; init; }

    public required ConnectionStringsSettings ConnectionStrings { get; init; }

    public required EmailsSettings Emails { get; init; }

    public required ErrorsSettings Errors { get; init; }

    public required GoogleSettings Google { get; init; }

    public required HangfireSettings Hangfire { get; init; }

    public required LoggingSettings Logging { get; init; }

    public required MembersSettings Members { get; init; }

    public required OAuthSettings OAuth { get; init; }

    public required PathSettings Paths { get; init; }

    public required PaymentsSettings Payments { get; init; }

    public required PlatformSettings[] Platforms { get; init; }

    public required RateLimitingSettings RateLimiting { get; init; }

    public required RecaptchaSettings Recaptcha { get; init; }

    public required ScheduledTasksSettings ScheduledTasks { get; init; }
}
