﻿using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ODK.Web.Common.Config.Settings;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Extensions.Logging;
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
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        IConfigurationRoot builtConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json")
            .Build();

        var logFileDirectory = builtConfig["Logging:Path"] ?? "";
        var connectionString = builtConfig["ConnectionStrings:Default"] ?? "";

        var outputTemplate = $"t:{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}}|ip:{{{IP}}}|u:{{{Name}}}|m:{{Message:lj}}|ex:{{Exception}}{{NewLine}}";
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
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
                .BetterStack(sourceToken: appSettings.BetterStack.SourceToken);
        }

        Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog(Logger);
    }
}
