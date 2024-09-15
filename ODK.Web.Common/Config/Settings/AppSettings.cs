namespace ODK.Web.Common.Config.Settings;

public class AppSettings
{
    public required AuthSettings Auth { get; set; }
    
    public required BetterStackSettings BetterStack { get; set; }

    public required EmailsSettings Emails { get; set; }

    public required ErrorsSettings Errors { get; set; }

    public required MembersSettings Members { get; set; }

    public required OAuthSettings OAuth { get; set; }

    public required PathSettings Paths { get; set; }

    public required PaymentsSettings Payments { get; set; }

    public required PlatformSettings[] Platforms { get; set; }

    public required RecaptchaSettings Recaptcha { get; set; }

    public required ScheduledTasksSettings ScheduledTasks { get; set; }
}
