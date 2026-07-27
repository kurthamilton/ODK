using NUnit.Framework;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Preflight guard for Stripe tests that depend on a completion webhook. Stripe delivers subscription /
/// payment webhooks to a single ngrok tunnel that forwards to the locally-running app (the app resolves
/// the platform from the webhook payload, so one tunnel serves both platforms) - an out-of-band, manual
/// dependency (the developer starts ngrok and points the Stripe sandbox webhook at it). Before a test
/// waits on such a webhook, it calls <see cref="EnsureReachable"/> to confirm the tunnel is up and reaches
/// the app; if not, the test fails immediately with an actionable message rather than hanging until timeout
/// waiting for a webhook that can never arrive.
/// </summary>
internal static class StripeWebhookTunnel
{
    private static readonly HttpClient Client = CreateClient();

    /// <summary>
    /// Confirms the configured webhook base URL (an ngrok tunnel) reaches the app homepage. Fails the test
    /// with a clear, actionable message when the URL is unconfigured, unreachable, or returns a non-success
    /// response.
    /// </summary>
    /// <param name="webhookBaseUrl">
    /// The ngrok tunnel base URL, from <c>E2ESettings.StripeWebhookBaseUrl</c> (may be blank).
    /// </param>
    public static async Task EnsureReachable(string webhookBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookBaseUrl))
        {
            Fail(webhookBaseUrl,
                "no URL is configured. Set 'Stripe:WebhookBaseUrl' (in the git-ignored " +
                "appsettings.Development.json / appsettings.local.json, or the ODK_E2E_Stripe__WebhookBaseUrl " +
                "env var) to your ngrok tunnel URL.");
        }

        try
        {
            using var response = await Client.GetAsync(webhookBaseUrl);
            if (!response.IsSuccessStatusCode)
            {
                Fail(webhookBaseUrl,
                    $"the tunnel returned {(int)response.StatusCode} {response.ReasonPhrase} (expected the app " +
                    "homepage). Is ngrok forwarding to the running app on the correct port?");
            }
        }
        catch (Exception ex) when (ex is not AssertionException)
        {
            Fail(webhookBaseUrl,
                $"the tunnel is not reachable: {ex.Message}. Start the ngrok tunnel that forwards to the local " +
                "app before running webhook-dependent Stripe tests.");
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        // ngrok shows a browser-warning interstitial to unknown user agents; this header skips it so the
        // request reaches the actual app rather than ngrok's warning page.
        client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
        return client;
    }

    private static void Fail(string webhookBaseUrl, string reason)
    {
        var url = string.IsNullOrWhiteSpace(webhookBaseUrl) ? "(unset)" : webhookBaseUrl;
        var message = $"Stripe webhook tunnel preflight failed [{url}]: {reason}";
        TestContext.Progress.WriteLine(message);
        Assert.Fail(message);
    }
}
