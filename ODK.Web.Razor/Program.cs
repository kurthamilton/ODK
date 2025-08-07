using System.Globalization;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ODK.Services.Tasks;
using ODK.Web.Common.Config;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Authentication;
using ODK.Web.Razor.Middleware;
using ODK.Web.Razor.Services;
using Serilog;

namespace ODK.Web.Razor;

public class Program
{
    public static void Main(string[] args)
    {
        var (app, appSettings) = BuildApp(args);

        // Configure the HTTP request pipeline.
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (!app.Environment.IsDevelopment())
        {
            // Do not use the .NET exception handling middleware - use ErrorHandlingMiddleware instead for more control
            // app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseWebOptimizer();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        var supportedCultures = new[]
        {
            new CultureInfo("en-GB")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-GB"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        app.Run();
    }

    private static WebApplicationBuilder AddHangfire(
        WebApplicationBuilder builder,
        AppSettings appSettings)
    {
        builder
            .Services
            .AddHangfire((provider, configuration) =>
            {
                BaseHangfireConfig(provider, configuration, appSettings.Hangfire);
                configuration.UseSqlServerStorage(
                    appSettings.ConnectionStrings.Default,
                    new SqlServerStorageOptions
                    {
                        SchemaName = appSettings.Hangfire.SchemaName
                    });
            });

        return builder;
    }

    private static void BaseHangfireConfig(
        IServiceProvider provider,
        IGlobalConfiguration configuration,
        HangfireSettings settings)
    {
        configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings();

        // Add job failure logging filter to log when jobs fail after all retry attempts
        configuration.UseFilter(new HangfireJobFailureLoggerAttribute());
        configuration.UseFilter(new AutomaticRetryAttribute
        {
            Attempts = settings.RetryAttempts
        });
    }

    private static (WebApplication, AppSettings) BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages().AddRazorPagesOptions(o =>
        {
            o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
        });

        builder.Services.AddControllers();
        
        builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
        builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        builder.Services.AddHttpContextAccessor();
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.EventsType = typeof(CustomCookieAuthenticationEvents);
            });
        builder.Services
            .AddScoped<IBackgroundTaskService, HangfireService>();

        var appSettings = AppStartup.ConfigureServices(builder.Configuration, builder.Services);
        LoggingConfig.Configure(builder, appSettings);

        builder.Services.AddWebOptimizer(pipeline =>
        {
            pipeline.AddCssBundle(
                route: "/css/odk.bundle.lib.css",
                "lib/font-awesome/css/all.css",
                "lib/flatpickr/dist/flatpickr.css",
                "lib/aspnet-client-validation/dist/aspnet-validation.css",
                "lib/slim-select/slimselect.css");
            pipeline.AddJavaScriptBundle(
                route: "/js/odk.bundle.js",
                "lib/cookieconsent/cookieconsent.min.js",
                "lib/bootstrap/js/bootstrap.bundle.js",
                "lib/flatpickr/dist/flatpickr.js",
                "lib/aspnet-client-validation/dist/aspnet-validation.js",
                "lib/slim-select/slimselect.js",
                "js/site.js",
                "js/odk.cookieconsent.js",
                "js/odk.dropdowns.js",
                "js/odk.forms.js",
                "js/odk.notifications.js",
                "js/odk.selects.js",
                "js/odk.tabs.js",
                "js/odk.topics.js",
                "js/odk.html-editor.js");
            pipeline.AddJavaScriptBundle(
                route: "/js/odk.bundle.head.js",
                "js/odk.global.js",
                "js/odk.themes.js");

            if (!builder.Environment.IsDevelopment())
            {
                // pipeline.MinifyCssFiles();
                // pipeline.MinifyJsFiles();
            }
        });

        AddHangfire(builder, appSettings);

        var app = builder.Build();
        return (app, appSettings);
    }
}