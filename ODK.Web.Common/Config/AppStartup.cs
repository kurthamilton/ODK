using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODK.Web.Common.Config.Settings;

namespace ODK.Web.Common.Config;

public static class AppStartup
{
    public static void ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddControllers().ConfigureJson();

        services.Configure<IISServerOptions>(options =>
        {
            options.AutomaticAuthentication = false;
        });

        AppSettings settings = GetAppSettings(config);
        services.ConfigureDependencies(config, settings);        
    }

    private static AppSettings GetAppSettings(IConfiguration config)
    {
        IConfigurationSection appSettingsSection = config.GetSection("AppSettings");
        return new AppSettings(appSettingsSection);
    }
}
