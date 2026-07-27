using Microsoft.Extensions.Configuration;

namespace ODK.E2E.Tests.Config;

/// <summary>
/// Settings for the end-to-end tests, read from appsettings.json (with a git-ignored
/// appsettings.Development.json / appsettings.local.json for secrets and overrides) and overridable by
/// <c>ODK_E2E_*</c> environment variables so the same tests can run locally or in CI.
/// </summary>
public static class E2ESettings
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
        .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables("ODK_E2E_")
        .Build();

    /// <summary>Connection string to the same database the app under test uses (to read activation tokens).</summary>
    public static string ConnectionString => GetRequired("ConnectionString");

    /// <summary>Base URL of the Default-platform instance (no trailing slash).</summary>
    public static string DefaultBaseUrl => GetRequired("DefaultBaseUrl").TrimEnd('/');

    /// <summary>Base URL of the DrunkenKnitwits-platform instance (no trailing slash).</summary>
    public static string DrunkenKnitwitsBaseUrl => GetRequired("DrunkenKnitwitsBaseUrl").TrimEnd('/');

    /// <summary>Stripe publishable key for the live payment settings the tests seed.</summary>
    public static string StripeApiPublicKey => GetRequired("Stripe:ApiPublicKey");

    /// <summary>Stripe secret key for the live payment settings the tests seed.</summary>
    public static string StripeApiSecretKey => GetRequired("Stripe:ApiSecretKey");

    /// <summary>
    /// A pre-onboarded Stripe sandbox connected account id (<c>acct_...</c>) used as the transfer
    /// destination for chapter-subscription purchases. Blank until a sandbox account has been onboarded;
    /// tests that seed a <c>ChapterPaymentAccount</c> for a real purchase require it.
    /// </summary>
    public static string StripeConnectedAccountId => GetOptional("Stripe:ConnectedAccountId");

    /// <summary>
    /// Base URL (an ngrok tunnel) Stripe delivers webhooks to. Blank when no
    /// tunnel is configured; webhook-dependent tests preflight it via <c>StripeWebhookTunnel</c>.
    /// </summary>
    public static string StripeWebhookBaseUrl => GetOptional("Stripe:WebhookBaseUrl").TrimEnd('/');

    private static string GetOptional(string key) => Configuration[key] ?? string.Empty;

    private static string GetRequired(string key)
        => Configuration[key]
            ?? throw new InvalidOperationException(
                $"E2E setting '{key}' is not configured. Set it in appsettings.json, appsettings.Development.json, appsettings.local.json, or the ODK_E2E_{key} environment variable.");
}