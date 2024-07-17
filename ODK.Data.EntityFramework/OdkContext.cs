using Microsoft.EntityFrameworkCore;
using ODK.Data.EntityFramework.Interceptors;

namespace ODK.Data.EntityFramework;
public class OdkContext : DbContext
{
    private readonly OdkContextSettings _settings;

    public OdkContext(OdkContextSettings settings)
    {
        _settings = settings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            .UseSqlServer(_settings.ConnectionString, options =>
            {
                options.EnableRetryOnFailure();
            })
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .AddInterceptors(new DebugInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OdkContext).Assembly);
    }
}
