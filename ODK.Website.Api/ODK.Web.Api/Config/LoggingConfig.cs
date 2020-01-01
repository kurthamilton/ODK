using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ODK.Web.Api.Config
{
    public static class LoggingConfig
    {
        private const string IP = "IP";
        private const string Name = "Name";

        public static ILogger Logger { get; private set; }

        public static LoggerProviderCollection Providers { get; } = new LoggerProviderCollection();

        public static void AddRequestProperties(HttpContext context)
        {
            string name = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "anonymous";
            LogContext.PushProperty(Name, name);

            string ip = context.Connection.RemoteIpAddress.ToString();
            LogContext.PushProperty(IP, !string.IsNullOrWhiteSpace(ip) ? ip : "unknown");
        }

        public static void Configure(string logFileDirectory, string connectionString)
        {
            string outputTemplate = $"t:{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}}|ip:{{{IP}}}|u:{{{Name}}}|m:{{Message:lj}}|ex:{{Exception}}{{NewLine}}";
            Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Logger(config => config
                    .Filter
                    .ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(path: Path.Combine(logFileDirectory, $"Errors.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: outputTemplate))
                .WriteTo.File(Path.Combine(logFileDirectory, $"Trace.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: outputTemplate)
                .WriteTo.Providers(Providers)
                .WriteTo.Console()
                .WriteTo.MSSqlServer(connectionString, "Logs")
                .CreateLogger();

            Log.Logger = Logger;
        }
    }
}
