using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODK.Web.Common.Config.Settings;
using Serilog;

namespace ODK.Web.Common.Config
{
    public static class AppStartup
    {
        private const string CorsPolicyName = "CorsPolicy";

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CorsPolicyName);

            app.RegisterExceptionHandling();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                LoggingConfig.AddRequestProperties(context);
                await next.Invoke();
            });

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static void ConfigureMapping(IServiceCollection services, params Type[] mappingTypes)
        {
            services.AddAutoMapper(mappingTypes);
        }

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
            services.ConfigureAuthentication(settings.Auth);
            services.ConfigureCors(CorsPolicyName, settings.Cors);
        }

        private static AppSettings GetAppSettings(IConfiguration config)
        {
            IConfigurationSection appSettingsSection = config.GetSection("AppSettings");
            return new AppSettings(appSettingsSection);
        }
    }
}
