using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ODK.Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogging(args);

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void ConfigureLogging(string[] args)
        {
            IConfigurationRoot builtConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();

            string path = builtConfig["Logging:Path"];
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Logger(config =>
                {
                    config
                    .Filter.ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Error)
                    .WriteTo.File(Path.Combine(path, "Errors.txt"));
                })
                .WriteTo.File(Path.Combine(path, "Trace.txt"))
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
