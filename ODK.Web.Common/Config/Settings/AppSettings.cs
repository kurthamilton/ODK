using Microsoft.Extensions.Configuration;

namespace ODK.Web.Common.Config.Settings
{
    public class AppSettings
    {
        public AppSettings(IConfigurationSection configuration)
        {
            Auth = configuration.Map<AuthSettings>("Auth");
            Members = configuration.Map<MembersSettings>("Members");
            Paths = configuration.Map<PathSettings>("Paths");
            Payments = configuration.Map<PaymentsSettings>("Payments");
            Recaptcha = configuration.Map<RecaptchaSettings>("Recaptcha");
            Urls = configuration.Map<UrlSettings>("Urls");
        }

        public AuthSettings Auth { get; }
        
        public MembersSettings Members { get; }

        public PathSettings Paths { get; }

        public PaymentsSettings Payments { get; }

        public RecaptchaSettings Recaptcha { get; }

        public UrlSettings Urls { get; }
    }
}
