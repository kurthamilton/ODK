using Microsoft.Extensions.Configuration;

namespace ODK.Web.Common.Config.Settings;

public class AppSettings
{
    public AppSettings(IConfigurationSection configuration)
    {
        Auth = configuration.Map<AuthSettings>("Auth");
        Emails = configuration.Map<EmailsSettings>("Emails");
        Errors = configuration.Map<ErrorsSettings>("Errors");
        Members = configuration.Map<MembersSettings>("Members");
        Paths = configuration.Map<PathSettings>("Paths");
        Payments = configuration.Map<PaymentsSettings>("Payments");
        Recaptcha = configuration.Map<RecaptchaSettings>("Recaptcha");
        ScheduledTasks = configuration.Map<ScheduledTasksSettings>("ScheduledTasks");
        Urls = configuration.Map<UrlSettings>("Urls");
    }

    public AuthSettings Auth { get; }
    
    public EmailsSettings Emails { get; }

    public ErrorsSettings Errors { get; }

    public MembersSettings Members { get; }

    public PathSettings Paths { get; }

    public PaymentsSettings Payments { get; }

    public RecaptchaSettings Recaptcha { get; }

    public ScheduledTasksSettings ScheduledTasks { get; }

    public UrlSettings Urls { get; }
}
