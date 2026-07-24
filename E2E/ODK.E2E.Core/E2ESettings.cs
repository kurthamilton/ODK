using Microsoft.Extensions.Configuration;

namespace ODK.E2E.Core;

/// <summary>
/// Settings for the end-to-end tests, read from appsettings.json (with an optional, git-ignored
/// appsettings.local.json for overrides) and overridable by <c>ODK_E2E_*</c> environment variables
/// so the same tests can run locally or in CI.
/// </summary>
public static class E2ESettings
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables("ODK_E2E_")
        .Build();

    /// <summary>The base URL of a running Group Squirrel instance (no trailing slash).</summary>
    public static string BaseUrl => GetRequired("BaseUrl").TrimEnd('/');

    /// <summary>Connection string to the same database the app under test uses (to read activation tokens).</summary>
    public static string ConnectionString => GetRequired("ConnectionString");

    private static string GetRequired(string key)
        => Configuration[key]
            ?? throw new InvalidOperationException(
                $"E2E setting '{key}' is not configured. Set it in appsettings.json, appsettings.local.json, or the ODK_E2E_{key} environment variable.");
}