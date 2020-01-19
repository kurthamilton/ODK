using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ODK.Web.Common.Config;
using Serilog;

namespace ODK.Web.Api.Admin
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog(providers: LoggingConfig.Providers);

        private static void ConfigureLogging(string[] args)
        {
            LoggingConfig.Configure(args);
        }
    }
}
