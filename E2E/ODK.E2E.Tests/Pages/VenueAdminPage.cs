using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin create-venue page (Default <c>/my/groups/{chapterId}/events/venues/new</c>,
/// DrunkenKnitwits <c>/{chapterName}/admin/events/venues/create</c>). The shared form is identical on
/// both platforms; only the URL differs, so the caller passes the platform-correct create path. On
/// success the app redirects to the venues list.
/// </summary>
internal class VenueAdminPage
{
    private readonly IPage _page;

    public VenueAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task CreateVenue(string createUrl, string name)
    {
        await _page.Navigate(createUrl);

        await _page.FillAsync("#Name", name);

        // Location is required (client-side). Set the name + lat/long directly, raising only a `change`
        // event so the Google Places autocomplete (which listens on focus/input) never fires a billable
        // Places call.
        await _page.EvalOnSelectorAsync(
            "#LocationName",
            "el => { el.value = 'London'; el.dispatchEvent(new Event('change', { bubbles: true })); }");
        await _page.EvalOnSelectorAsync("#Lat", "el => el.value = '51.5074'");
        await _page.EvalOnSelectorAsync("#Long", "el => el.value = '-0.1278'");

        await _page.ClickAsync("button:has-text('Create')");

        // Success redirects to the venues list (path ends '/venues'); the create page ends '/new' or
        // '/create'. Staying put means a re-rendered validation error - surface it.
        try
        {
            await _page.WaitForURLAsync(
                url => new Uri(url).AbsolutePath.TrimEnd('/').EndsWith("/venues"),
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create venue did not redirect to the venues list. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }
}
