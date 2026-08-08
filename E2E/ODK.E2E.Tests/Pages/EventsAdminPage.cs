using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin events list (Default <c>/my/groups/{chapterId}/events</c>, DrunkenKnitwits
/// <c>/{chapterName}/admin/events</c>) and its venue filter. The filter travels in the query string as
/// the venue's <em>slug</em> rather than its id, so the URL stays readable.
/// </summary>
internal class EventsAdminPage
{
    private readonly IPage _page;

    public EventsAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Opens the list, optionally filtered to one venue, and returns the rendered table body. Asserting
    /// on the text rather than a column position keeps this from breaking when the table gains a column;
    /// event names are unique per run, so containment is a precise enough check.
    /// </summary>
    public async Task<string> GetEventsTableText(string eventsAdminUrl, string? venueSlug = null)
    {
        var url = string.IsNullOrEmpty(venueSlug)
            ? eventsAdminUrl
            : $"{eventsAdminUrl}?venue={venueSlug}";

        await _page.Navigate(url);

        return await _page.InnerTextAsync("table tbody");
    }
}
