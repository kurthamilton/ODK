using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class AppSettings
{
    public required AuthSettings Auth { get; init; }

    public required BetterStackSettings BetterStack { get; init; }

    public required BrevoSettings Brevo { get; init; }

    public required ConnectionStringsSettings ConnectionStrings { get; init; }

    public required EmailsSettings Emails { get; init; }

    public required EnvironmentType Environment { get; init; }

    public required EventsSettings Events { get; init; }

    public required GoogleSettings Google { get; init; }

    public required GroupsSettings Groups { get; init; }

    public required HangfireSettings Hangfire { get; init; }

    public required HibpSettings Hibp { get; init; }

    public required ImagingSettings Imaging { get; init; }

    public required InstagramSettings Instagram { get; init; }

    public required LocalisationSettings Localisation { get; init; }

    public required LoggingSettings Logging { get; init; }

    public required MembersSettings Members { get; init; }

    public required OAuthSettings OAuth { get; init; }

    public required PaymentsSettings Payments { get; init; }

    public required PayPalSettings PayPal { get; init; }

    /// <summary>The platform this deployment serves. Every environment states its own.</summary>
    public required PlatformType Platform { get; init; }

    public required Dictionary<PlatformType, PlatformSettings> Platforms { get; init; }

    public required RateLimitingSettings RateLimiting { get; init; }

    public required RecaptchaSettings Recaptcha { get; init; }

    public required ReoonSettings Reoon { get; init; }

    public required ScheduledTasksSettings ScheduledTasks { get; init; }

    public required StripeSettings Stripe { get; init; }

    public required SubscriptionsSettings Subscriptions { get; init; }

    public required WhatsAppSettings WhatsApp { get; init; }
}