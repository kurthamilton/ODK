using Microsoft.Playwright;
using ODK.E2E.Core;

namespace ODK.E2E.Tests.Pages;

internal static class PageExtensions
{
    internal static Task Navigate(this IPage page, string path)
        => page.GotoAsync($"{E2ESettings.BaseUrl}{path}");
}