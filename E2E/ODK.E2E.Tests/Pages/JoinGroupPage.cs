using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-facing join page (<c>/groups/{slug}/join</c>). Requires the member to be logged in and
/// the group to be approved and published; on success the app redirects back to the group home page.
/// </summary>
internal class JoinGroupPage
{
    private readonly IPage _page;

    public JoinGroupPage(IPage page)
    {
        _page = page;
    }

    public async Task JoinAsync(string slug)
    {
        await _page.Navigate($"/groups/{slug}/join");

        var tokenCount = await _page.Locator("form input[name='__RequestVerificationToken']").CountAsync();

        var response = await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Join group')"),
            r => r.Request.Method == "POST" && r.Url.Contains("/join"));

        // Success redirects to the group home page (away from .../join).
        try
        {
            await _page.WaitForURLAsync(
                url => url.Contains($"/groups/{slug}") && !url.Contains("/join"),
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            await _page.WaitForLoadStateAsync();
            var alerts = await _page.Locator(".alert, [role='alert']").AllInnerTextsAsync();
            throw new InvalidOperationException(
                $"Join did not redirect. postStatus={response.Status}, tokenInForm={tokenCount}, URL='{_page.Url}'. Alerts=[{string.Join(" | ", alerts)}].");
        }
    }
}