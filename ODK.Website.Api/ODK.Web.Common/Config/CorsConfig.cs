using Microsoft.Extensions.DependencyInjection;
using ODK.Web.Common.Config.Settings;

namespace ODK.Web.Common.Config
{
    public static class CorsConfig
    {
        public static void ConfigureCors(this IServiceCollection services, string policyName, CorsSettings settings)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName,
                builder =>
                {
                    builder.WithHeaders(settings.AllowedHeaders.Split(','));
                    builder.WithMethods(settings.AllowedMethods.Split(','));
                    builder.WithOrigins(settings.AllowedOrigins.Split(','));
                });
            });
        }
    }
}
