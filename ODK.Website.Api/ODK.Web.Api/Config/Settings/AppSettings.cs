using Microsoft.Extensions.Configuration;

namespace ODK.Web.Api.Config.Settings
{
    public class AppSettings
    {
        public AppSettings(IConfigurationSection configuration)
        {
            Auth = configuration.Map<AuthSettings>("Auth");
            Cors = configuration.Map<CorsSettings>("Cors");
            Members = configuration.Map<MembersSettings>("Members");
            Urls = configuration.Map<UrlSettings>("Urls");
        }

        public AuthSettings Auth { get; }

        public CorsSettings Cors { get; }

        public MembersSettings Members { get; }

        public UrlSettings Urls { get; }
    }
}
