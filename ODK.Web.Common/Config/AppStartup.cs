using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODK.Web.Common.Config.Settings;

namespace ODK.Web.Common.Config;

public static class AppStartup
{
    public static AppSettings ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddControllers().ConfigureJson();
        services.AddHttpContextAccessor();
        services.AddHttpClient();

        services.Configure<IISServerOptions>(options =>
        {
            options.AutomaticAuthentication = false;
        });

        AppSettings settings = GetAppSettings(config);
        services.ConfigureDependencies(config, settings);

        return settings;
    }

    private static AppSettings GetAppSettings(IConfiguration config)
    {
        return config.Get<AppSettings>() ?? throw new Exception("Error parsing app settings");
    }
}
