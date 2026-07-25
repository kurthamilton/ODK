using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The member-facing upcoming-events listing page (Default <c>/groups/{slug}/events</c>,
/// DrunkenKnitwits <c>/{chapterName}/events</c>). Each listed event renders as a link to its detail page
/// (<c>.../events/{shortcode}</c>), so presence is detected by that link. The listing filters events by
/// the viewer's visibility, so which events appear depends on who is (or isn't) logged in.
/// </summary>
internal class EventsListPage
{
    private readonly IPage _page;

    public EventsListPage(IPage page)
    {
        _page = page;
    }

    public async Task<bool> IsEventListed(string eventsListUrl, string shortcode)
    {
        await _page.Navigate(eventsListUrl);
        await _page.WaitForLoadStateAsync();

        return await _page.Locator($"a[href*='/events/{shortcode}']").CountAsync() > 0;
    }
}
