using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ODK.Web.Api.Config;
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog(providers: LoggingConfig.Providers);

        private static void ConfigureLogging(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationRoot builtConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json")
                .AddCommandLine(args)
                .Build();

            string path = builtConfig["Logging:Path"];
            string connectionString = builtConfig["ConnectionStrings:Default"];
            LoggingConfig.Configure(path, connectionString);
        }
    }
}
