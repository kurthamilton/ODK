using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODK.Web.Api.Config;
using ODK.Web.Api.Config.Settings;

namespace ODK.Web.Api
{
    public class Startup
    {
        private const string CorsPolicyName = "CorsPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddControllers().ConfigureJson();
            services.ConfigureMapping(typeof(MappingConfig));

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            AppSettings settings = GetAppSettings();
            services.ConfigureDependencies(Configuration, settings);
            services.ConfigureAuthentication(settings.Auth);
            services.ConfigureCors(CorsPolicyName, settings.Cors);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CorsPolicyName);

            app.RegisterExceptionHandling();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                LoggingConfig.AddRequestProperties(context);
                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private AppSettings GetAppSettings()
        {
            IConfigurationSection appSettingsSection = Configuration.GetSection("AppSettings");
            return new AppSettings(appSettingsSection);
        }
    }
}
