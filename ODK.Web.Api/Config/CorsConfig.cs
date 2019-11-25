using Microsoft.Extensions.DependencyInjection;

namespace ODK.Web.Api.Config
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
                    builder.WithOrigins(settings.AllowedOrigins.Split('.'));
                });
            });
        }
    }
}
