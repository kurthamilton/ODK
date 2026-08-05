using Microsoft.EntityFrameworkCore.Design;
using ODK.Data.EntityFramework;

namespace ODK.Web.Razor;

// Design-time only. Lets `dotnet ef` and the migration bundle construct OdkContext WITHOUT building the full
// web host (Hangfire, Serilog, auth, ...), whose services need runtime config that isn't present when building
// or deploying. EF and efbundle prefer this factory over the host, so migrations no longer depend on the host
// building cleanly. Connection string resolution: environment variables (CI sets ConnectionStrings__Default)
// then appsettings[.{env}].json (local) then a non-connecting placeholder (enough to read the model when
// bundling; the real target is supplied via `--connection` / the env var at apply time).
public class OdkContextFactory : IDesignTimeDbContextFactory<OdkContext>
{
    public OdkContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=placeholder;Database=placeholder;";
        }

        return new OdkContext(new OdkContextSettings(connectionString));
    }
}
