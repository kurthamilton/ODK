using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

internal static class PageExtensions
{
    /// <summary>
    /// Navigates to a relative path. The absolute host comes from the browser context's
    /// <c>BaseURL</c> (set per platform by the test's base class / by provisioning), so the same page
    /// objects work against whichever platform the fixture targets.
    /// </summary>
    internal static Task Navigate(this IPage page, string path)
        => page.GotoAsync(path);
}
