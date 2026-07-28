using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// A thin helper for member-facing page smoke checks: navigates to a URL and returns the HTTP status
/// (Playwright doesn't throw on 4xx/5xx), then lets a test assert an expected element rendered, so a
/// page can be checked for "returned 200 and rendered its content".
/// </summary>
internal sealed class MemberFacingPage
{
    private readonly IPage _page;

    public MemberFacingPage(IPage page)
    {
        _page = page;
    }

    /// <summary>True if an element matching the (Playwright) selector is present and visible.</summary>
    public Task<bool> IsVisible(string selector) => _page.Locator(selector).First.IsVisibleAsync();

    /// <summary>Navigates to the URL and returns the HTTP status.</summary>
    public async Task<int> Open(string url)
    {
        var response = await _page.GotoAsync(url)
            ?? throw new InvalidOperationException($"No response navigating to '{url}'.");
        return response.Status;
    }
}
