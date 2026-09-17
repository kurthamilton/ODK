using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin create-venue page (Default <c>/my/groups/{slug}/events/venues/new</c>,
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

    /// <param name="externalId">
    /// A real Google place id. The server resolves it for itself, so it has to be one Google still knows -
    /// the venue's own name and slug come from that lookup, and <paramref name="name"/> is only what this
    /// group calls it. Two venues in one group need two different places, or the second resolves to the
    /// first and is refused.
    /// </param>
    public async Task CreateVenue(string createUrl, string name, string externalId)
    {
        await _page.Navigate(createUrl);

        await _page.FillAsync("#Name", name);

        /* Set directly, raising only a `change` event, so the autocomplete - which listens on focus and
           input - never fires a billable Places call from the browser. The server still makes one. */
        await _page.EvalOnSelectorAsync(
            "#ExternalId",
            "el => { el.value = '" + externalId + "'; el.dispatchEvent(new Event('change', { bubbles: true })); }");

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
