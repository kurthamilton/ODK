using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ODK.Remote.Web.Api.Auth;
using ODK.Remote.Web.Api.Config;
using ODK.Remote.Web.Api.Config.Settings;

namespace ODK.Remote.Web.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            AppSettings settings = GetAppSettings();
            services.ConfigureDependencies(settings.Paths);

            services.AddSingleton(settings.Auth);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private AppSettings GetAppSettings()
        {
            AppSettings appSettings = new AppSettings();
            Configuration.Bind("AppSettings", appSettings);
            return appSettings;
        }
    }
}
