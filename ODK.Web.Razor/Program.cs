using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ODK.Core.Platforms;
using ODK.Infrastructure;
using ODK.Infrastructure.Settings;
using ODK.Services.Payments;
using ODK.Services.Platforms;
using ODK.Services.Tasks;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Authentication;
using ODK.Web.Razor.Hubs;
using ODK.Web.Razor.Middleware;
using ODK.Web.Razor.Mvc;
using ODK.Web.Razor.Services;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;

namespace ODK.Web.Razor;

public class Program
{
    public static void Main(string[] args)
    {
        var (app, appSettings) = BuildApp(args);

        /* The site serves either way - an unstated platform is read as Drunken Knitwits when the settings
           are mapped - so the log is the only place that can say the deployment never stated one. */
        if (appSettings.Platform == PlatformType.None)
        {
            app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(Program).FullName!)
                .LogWarning(
                    "Platform is not stated in configuration - serving {Platform}",
                    app.Services.GetRequiredService<PlatformProviderSettings>().Platform);
        }

        // Pin the app-wide default culture so model binding parses posted values (dates, decimals) under a
        // fixed culture regardless of host. The request locale is applied for *rendering only* by
        // RequestCultureResultFilter, so display formatting follows the request without affecting parsing.
        var defaultCulture = new CultureInfo(appSettings.Localisation.DefaultLocale);
        CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

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

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter()]
        });

        app.MapRazorPages();
        app.MapControllers();

        /* The chapter is a route value so RequestStore resolves it the way it does for a controller, and a
           hub method can compose a chapter request without a lookup of its own.

           Two mappings rather than one {chapterId:guid?}, because MapHub also maps path + "/negotiate": an
           optional parameter followed by another segment is not optional, so a connection with no chapter
           would fail to negotiate. Both reach the same hub type, and groups belong to the type rather than
           to the path, so a broadcast reaches a connection made on either.

           The request store middleware is skipped: a hub invocation gets its own scope, so the store that
           middleware loads is never the one a hub method holds, and a socket handshake should not pay a
           load nothing reads. PaymentsHub loads its own. */
        app.MapHub<PaymentsHub>("/hubs/payments")
            .WithMetadata(new SkipRequestStoreMiddlewareAttribute());
        app.MapHub<PaymentsHub>("/hubs/payments/{chapterId:guid}")
            .WithMetadata(new SkipRequestStoreMiddlewareAttribute());

        app.MapGet("/favicon.ico", async (HttpContext ctx, IPlatformProvider platformProvider) =>
        {
            var file = platformProvider.Platform == PlatformType.DrunkenKnitwits
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
                            SchemaName = ServedPlatform
                                .Of(appSettings, appSettings.Hangfire.Platforms)
                                .SchemaName
                        });
                }
            })
            .AddHangfireServer(options =>
            {
                options.Queues = Enum.GetValues<BackgroundTaskQueueType>()
                    .Where(x => x != BackgroundTaskQueueType.None)
                    .Select(x => x.ToString().ToLowerInvariant())
                    .ToArray();
                options.WorkerCount = appSettings.Hangfire.WorkerCount;
            });

        return builder;
    }

    private static void BaseHangfireConfig(
        IServiceProvider provider,
        IGlobalConfiguration configuration,
        HangfireSettings settings)
    {
        /* These two decide the wire format of every queued job's arguments, so BackgroundJobPayloadTests
           applies the same pair before asserting what that format is. Changing either changes how jobs
           already in the queue are read, and the test has to move with it. */
        configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings();

        // Add job failure logging filter to log when jobs fail after all retry attempts
        configuration.UseFilter(
            new HangfireJobFailureLoggerAttribute(provider.GetRequiredService<IServiceScopeFactory>()));
        configuration.UseFilter(new AutomaticRetryAttribute
        {
            Attempts = settings.RetryAttempts
        });
    }

    private static (WebApplication, AppSettings) BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var mvcBuilder = builder.Services.AddRazorPages();

        // In Development, recompile Razor views at runtime so a .cshtml edit shows on the next request (a
        // browser refresh) without a rebuild or restart. dotnet watch's Razor hot reload is unreliable under
        // the concurrently dev setup, so this is the dependable path for view changes. Not enabled elsewhere.
        if (builder.Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

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

        builder.Services
            .AddScoped<IPaymentUpdateBroadcaster, SignalRPaymentUpdateBroadcaster>();

        builder.Services
            .AddScoped<IMemberImportStagingService, MemberImportStagingService>()
            .AddScoped<IMemberImportPreviewBuilder, MemberImportPreviewBuilder>();

        builder.Services.AddLocalization();

        AddLogging(builder, appSettings);

        // register the [OdkInject] attribute for dependency injection in PageModel classes
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IPageModelActivatorProvider, InjectingPageModelActivatorProvider<OdkInjectAttribute>>());

        AddHangfire(builder, appSettings);

        var app = builder.Build();
        return (app, appSettings);
    }

    private static void AddLogging(WebApplicationBuilder builder, AppSettings appSettings)
    {
        const string IP = "IP";
        const string Name = "Name";

        // Serilog swallows sink failures (e.g. an unwritable log directory) by default; route them to stderr
        // so a misconfigured Logging:Path surfaces in the host stdout log instead of vanishing.
        SelfLog.Enable(Console.Error);

        // required in order for app.UseSerilogRequestLogging to work,
        // which uses more condensed request logging instead of asp.net's "spammy" version
        builder.Services.AddSerilog();

        var logFileDirectory = ServedPlatform.Of(appSettings, appSettings.Logging.Platforms).Path;
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

        /* A source is its token *and* the host that ingests it, so both have to be stated for the sink to be
           configured at all - either one blank reads as unstated and leaves it off. Not a token alone with
           some default endpoint: the sink reports a delivery failure to SelfLog and nowhere else, so a token
           posted to the wrong host is logs that silently never arrive. */
        var betterStack = ServedPlatform.Of(appSettings, appSettings.BetterStack.Platforms);

        if (!string.IsNullOrEmpty(betterStack.SourceToken) && !string.IsNullOrEmpty(betterStack.IngestingHost))
        {
            loggerConfiguration = loggerConfiguration
                .WriteTo
                .BetterStack(
                    sourceToken: betterStack.SourceToken,
                    betterStackEndpoint: $"https://{betterStack.IngestingHost}")
                .Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.EndsWith(".css")))
                .Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.EndsWith(".js")))
                /* A hub connection is one request held open for as long as the page is, so it logs once with
                   an elapsed time measured in minutes - which reads as a slow request and skews everything
                   BetterStack is looked at for. The local trace file keeps them. */
                .Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", v => v.StartsWith("/hubs/")));
        }

        var logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog(logger);
        builder.Services.AddSingleton(logger);
    }

    private static AppSettings ConfigureServices(IConfiguration config, IServiceCollection services)
    {
        // Validate an antiforgery token on every unsafe request (POST/PUT/PATCH/DELETE), for both MVC
        // controllers and Razor Page handlers. The header name lets AJAX POSTs send the token via header
        // (see the request-verification-token meta tag in the layout). Endpoints that receive external
        // POSTs (webhooks, scheduled-task cron, OAuth callbacks) opt out with [IgnoreAntiforgeryToken].
        services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

        // Decorate IAntiforgery so CSRF validation failures are logged (the filter otherwise swallows them
        // into a bare 400 that never reaches middleware, and the framework only logs at Information).
        var antiforgeryDescriptor = services.Last(x => x.ServiceType == typeof(IAntiforgery));
        services.Remove(antiforgeryDescriptor);
        services.AddSingleton<IAntiforgery>(sp =>
        {
            var inner = (IAntiforgery)(antiforgeryDescriptor.ImplementationInstance
                ?? antiforgeryDescriptor.ImplementationFactory?.Invoke(sp)
                ?? ActivatorUtilities.CreateInstance(sp, antiforgeryDescriptor.ImplementationType!));
            return new LoggingAntiforgery(inner, sp.GetRequiredService<ILogger<LoggingAntiforgery>>());
        });

        services
            .AddMemoryCache()
            .AddControllers(options =>
            {
                options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
                options.Filters.Add<AntiforgeryValidationFailedResultFilter>(
                    AntiforgeryValidationFailedResultFilter.FilterOrder);
                options.Filters.Add<RequestCultureResultFilter>();
            })
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