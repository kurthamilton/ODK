using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace ODK.Web.Api.Config
{
    public static class LoggingConfig
    {
        private const string IP = "IP";
        private const string Name = "Name";

        public static void AddRequestProperties(HttpContext context)
        {
            string name = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "anonymous";
            LogContext.PushProperty(Name, name);

            string ip = context.Connection.RemoteIpAddress.ToString();
            LogContext.PushProperty(IP, !string.IsNullOrWhiteSpace(ip) ? ip : "unknown");
        }

        public static void Configure(string logFileDirectory)
        {
            string OutputTemplate = $"t:{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}}|ip:{{{IP}}}|u:{{{Name}}}|m:{{Message:lj}}|ex:{{Exception}}{{NewLine}}";
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(config => config
                    .Filter
                    .ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Error)
                    .WriteTo.File(path: Path.Combine(logFileDirectory, $"Errors.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: OutputTemplate))
                .WriteTo.File(Path.Combine(logFileDirectory, $"Trace.{DateTime.Today:yyyyMMdd}.txt"), outputTemplate: OutputTemplate)
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
