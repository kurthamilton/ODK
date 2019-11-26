using Microsoft.Extensions.Configuration;

namespace ODK.Web.Api.Config
{
    public class AppSettings
    {
        public AppSettings(IConfigurationSection configuration)
        {
            Auth = configuration.Map<AuthSettings>("Auth");
            Cors = configuration.Map<CorsSettings>("Cors");
            Urls = configuration.Map<UrlSettings>("Urls");
        }

        public AuthSettings Auth { get; }

        public CorsSettings Cors { get; }

        public UrlSettings Urls { get; }
    }
}
