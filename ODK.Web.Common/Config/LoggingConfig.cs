using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ODK.Web.Common.Config.Settings;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;

namespace ODK.Web.Common.Config;

public static class LoggingConfig
{
    private const string IP = "IP";
    private const string Name = "Name";

    public static ILogger? Logger { get; private set; }

    public static LoggerProviderCollection Providers { get; } = new();

    public static void AddRequestProperties(HttpContext context)
    {
        string? name = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "anonymous";
        LogContext.PushProperty(Name, name);

        string? ip = context.Connection.RemoteIpAddress?.ToString();
        LogContext.PushProperty(IP, !string.IsNullOrWhiteSpace(ip) ? ip : "unknown");
    }

    public static void Configure(WebApplicationBuilder builder, AppSettings appSettings)
    {
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
            .WriteTo.Providers(Providers)
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

        Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog(Logger);
    }
}