using Microsoft.Extensions.Configuration;

namespace ODK.Web.Api.Config
{
    public class AppSettings
    {
        public AppSettings(IConfigurationSection configuration)
        {
            Auth = configuration.Map<AuthSettings>("Auth");
            Cors = configuration.Map<CorsSettings>("Cors");
        }

        public AuthSettings Auth { get; }

        public CorsSettings Cors { get; }
    }
}
