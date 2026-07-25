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

    /// <summary>
    /// Fills the given chapter-property answers on the join form and submits. Returns true if the join
    /// succeeded (redirected off the join page), false if it was blocked client-side (e.g. a required
    /// property was left unanswered) - so callers can assert either outcome without a thrown timeout.
    /// </summary>
    public async Task<bool> TryJoinWithProperties(string slug, IReadOnlyDictionary<Guid, string> propertyAnswers)
    {
        await _page.Navigate($"/groups/{slug}/join");

        foreach (var (chapterPropertyId, value) in propertyAnswers)
        {
            await _page.FillChapterProperty(chapterPropertyId, value);
        }

        await _page.ClickAsync("button:has-text('Join group')");

        try
        {
            await _page.WaitForURLAsync(
                url => url.Contains($"/groups/{slug}") && !url.Contains("/join"),
                new() { Timeout = 10000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task Join(string slug)
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