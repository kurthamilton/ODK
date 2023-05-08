using Microsoft.Extensions.Configuration;

namespace ODK.Web.Common.Config.Settings
{
    public class AppSettings
    {
        public AppSettings(IConfigurationSection configuration)
        {
            Auth = configuration.Map<AuthSettings>("Auth");
            Cors = configuration.Map<CorsSettings>("Cors");
            Members = configuration.Map<MembersSettings>("Members");
            Paths = configuration.Map<PathSettings>("Paths");
            Payments = configuration.Map<PaymentsSettings>("Payments");
            Urls = configuration.Map<UrlSettings>("Urls");
        }

        public AuthSettings Auth { get; }

        public CorsSettings Cors { get; }

        public MembersSettings Members { get; }

        public PathSettings Paths { get; }

        public PaymentsSettings Payments { get; }

        public UrlSettings Urls { get; }
    }
}
