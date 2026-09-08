using Microsoft.Playwright;
using ODK.E2E.Tests.Config;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Runs one of the app's cron tasks (<c>ScheduledTasksController</c>) by posting to its endpoint, which is
/// the only way to run one: nothing in the UI triggers them and the app runs none on a timer of its own.
/// They are authenticated by the ScheduledTasks API key rather than by a session, so the page passed in is
/// only borrowed for its request context and need not be logged in.
/// </summary>
internal static class ScheduledTasks
{
    /// <summary>
    /// Runs the sweep that moves members whose site subscription has lapsed past the cooldown onto the
    /// platform's default plan.
    /// <para>
    /// It sweeps the whole platform, not only the caller's member, so a fixture that runs it has to be
    /// <c>[NonParallelizable]</c> - otherwise it downgrades whatever another fixture has lapsed, underneath
    /// that fixture's own assertions.
    /// </para>
    /// </summary>
    public static Task DowngradeLapsedSiteSubscriptions(IPage page, string baseUrl)
        => Post(page, baseUrl, "/scheduledtasks/subscriptions/lapsed/downgrade");

    private static async Task Post(IPage page, string baseUrl, string path)
    {
        var response = await page.Context.APIRequest.PostAsync(
            $"{baseUrl}{path}",
            new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    [E2ESettings.ScheduledTasksApiKeyHeader] = E2ESettings.ScheduledTasksApiKey
                },
                MaxRedirects = 0
            });

        /* Every one of these endpoints swallows what its task threw and answers 200 regardless, so this says
           only that the request authenticated and reached the action - what the task did is the caller's own
           assertions. A failure here is the API key: the app's configuration and this solution's have to
           state the same one. */
        if (!response.Ok)
        {
            throw new InvalidOperationException(
                $"Scheduled task '{path}' answered {response.Status}. The app's ScheduledTasks:ApiKey and " +
                "ApiKeyHeader have to match the ones in ODK.E2E.Tests/appsettings.json.");
        }
    }
}
