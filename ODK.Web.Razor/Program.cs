using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ODK.Core.Platforms;
using ODK.Infrastructure;
using ODK.Infrastructure.Settings;
using ODK.Services.Tasks;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Authentication;
using ODK.Web.Razor.Middleware;
using ODK.Web.Razor.Services;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;

namespace ODK.Web.Razor;

public class Program
{
    public static void Main(string[] args)
    {
        var (app, appSettings) = BuildApp(args);

        // Configure the HTTP request pipeline.
        app
            .UseMiddleware<HttpContextLoggingMiddleware>()
            .UseMiddleware<RateLimitingMiddleware>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .UseRouting()
            .UseAuthentication()
            .UseMiddleware<RequestStoreMiddleware>()
            .UseAuthorization()
            .UseSerilogRequestLogging();

        if (!app.Environment.IsDevelopment())
        {
            // Do not use the .NET exception handling middleware - use ErrorHandlingMiddleware instead for more control
            // app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            // show all registered endpoints
            app.MapGet("/_endpoints", (IEnumerable<EndpointDataSource> sources) =>
            {
                return sources
                    .SelectMany(s => s.Endpoints)
                    .OfType<RouteEndpoint>()
                    .Select(e => new
                    {
                        e.RoutePattern.RawText,
                        Methods = e.Metadata
                            .OfType<HttpMethodMetadata>()
                            .FirstOrDefault()?.HttpMethods
                    });
            });
        }

        app.UseWebOptimizer();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });

        app.MapRazorPages();
        app.MapControllers();

        var defaultCulture = new CultureInfo("en-GB");
        var supportedCultures = new[]
        {
            defaultCulture
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture.Name),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        app.MapGet("/favicon.ico", async (HttpContext ctx, IPlatformProvider platformProvider) =>
        {
            var platform = platformProvider.GetPlatform(ctx.Request.GetDisplayUrl());

            var file = platform == PlatformType.DrunkenKnitwits
                ? "wwwroot/assets/drunkenknitwits/favicon/favicon.ico"
                : "wwwroot/assets/groupsquirrel/favicon/favicon.ico";

            ctx.Response.ContentType = "image/x-icon";
            await ctx.Response.SendFileAsync(file);
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

                if (appSettings.Hangfire.InMemory)
                {
                    configuration.UseInMemoryStorage();
                }
                else
                {
                    configuration.UseSqlServerStorage(
                        appSettings.ConnectionStrings.Default,
                        new SqlServerStorageOptions
                        {
                            SchemaName = appSettings.Hangfire.SchemaName
                        });
                }
            })
            .AddHangfireServer(options =>
            {
                options.WorkerCount = appSettings.Hangfire.WorkerCount;
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
        builder.Services.AddHttpContextAccessor();
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/error/403";
                options.EventsType = typeof(CustomCookieAuthenticationEvents);
            });

        var appSettings = ConfigureServices(builder.Configuration, builder.Services);

        builder.Services
            .AddScoped<IBackgroundTaskService, HangfireService>();

        builder.Services.AddLocalization();

        AddLogging(builder, appSettings);

        // register the [OdkInject] attribute for dependency injection in PageModel classes
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPageModelActivatorProvider, InjectingPageModelActivatorProvider<OdkInjectAttribute>>());

        builder.Services.AddWebOptimizer(pipeline =>
        {
            pipeline.AddCssBundle(
                route: "/css/odk.bundle.lib.css",
                "lib/font-awesome/css/all.css",
                "lib/flatpickr/flatpickr.css",
                "lib/aspnet-client-validation/dist/aspnet-validation.css",
                "lib/slim-select/slimselect.css");
            pipeline.AddJavaScriptBundle(
                route: "/js/odk.bundle.js",
                "lib/cookieconsent/cookieconsent.min.js",
                "lib/bootstrap/js/bootstrap.bundle.js",
                "lib/flatpickr/flatpickr.js",
                "lib/aspnet-client-validation/dist/aspnet-validation.js",
                "lib/jscolor/jscolor.js",
                "lib/slim-select/slimselect.js",
                "js/odk.js",
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

    private static void AddLogging(WebApplicationBuilder builder, AppSettings appSettings)
    {
        const string IP = "IP";
        const string Name = "Name";

        // required in order for app.UseSerilogRequestLogging to work,
        // which uses more condensed request logging instead of asp.net's "spammy" version
        builder.Services.AddSerilog();

        var logFileDirectory = appSettings.Logging.Path;
        var connectionString = appSettings.ConnectionStrings.Default;

        var outputTemplate = $"t:{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}}|ip:{{{IP}}}|u:{{{Name}}}|m:{{Message:lj}}|ex:{{Exception}}{{NewLine}}";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.WithClientIp()
            .Enrich.WithProperty("ContentRootPath", builder.Environment.ContentRootPath)
            .Enrich.FromLogContext()
            .WriteTo.Logger(config => config
                .Filter
                .ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                .WriteTo.File(path: Path.Combine(logFileDirectory, $"Errors.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: outputTemplate)
                .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions
                {
                    TableName = "Logs"
                })
            )
            .WriteTo.File(Path.Combine(logFileDirectory, $"Trace.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: outputTemplate)
            .WriteTo.Console();

        if (!string.IsNullOrEmpty(appSettings.BetterStack.SourceToken))
        {
            loggerConfiguration = loggerConfiguration
                .WriteTo
                .BetterStack(sourceToken: appSettings.BetterStack.SourceToken)
                .Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.EndsWith(".css")))
                .Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.EndsWith(".js")));
        }

        var logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog(logger);
        builder.Services.AddSingleton(logger);
    }

    private static AppSettings ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        services
            .AddMemoryCache()
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        services
            .AddHttpContextAccessor()
            .AddHttpClient()
            .Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

        var appSettings = config.Get<AppSettings>() ?? throw new Exception("Error parsing app settings");
        services.ConfigureDependencies(appSettings);

        return appSettings;
    }
}