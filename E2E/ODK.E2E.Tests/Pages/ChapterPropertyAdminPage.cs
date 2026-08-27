using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The admin area for a chapter's member-profile properties (the per-chapter questions members answer).
/// Create is at Default <c>/my/groups/{chapterId}/members/properties/new</c> / DrunkenKnitwits
/// <c>/{chapterName}/admin/members/properties/create</c>; the list page carries per-row move-up/move-down
/// reorder controls. The forms are identical on both platforms; the caller passes the platform-correct
/// URL.
/// </summary>
internal class ChapterPropertyAdminPage
{
    private readonly IPage _page;

    public ChapterPropertyAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task CreateProperty(string createUrl, string label, bool required = false, bool applicationOnly = false)
    {
        await _page.Navigate(createUrl);

        // Name is the internal identifier, Label is the displayed question; both are required. DataType
        // defaults to Text, which is what we want.
        await _page.FillAsync("#Name", label);
        await _page.FillAsync("#Label", label);

        if (required)
        {
            await _page.CheckAsync("#Required");
        }

        if (applicationOnly)
        {
            await _page.CheckAsync("#ApplicationOnly");
        }

        await _page.ClickAsync("button:has-text('Create')");

        // Success redirects to the properties list (path ends '/properties'); the create page ends
        // '/new' or '/create'. Staying put means a re-rendered validation error - surface it.
        try
        {
            await _page.WaitForURLAsync(
                url => new Uri(url).AbsolutePath.TrimEnd('/').EndsWith("/properties"),
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var errors = await _page.Locator(
                ".field-validation-error, .text-danger, .validation-summary-errors, .alert").AllInnerTextsAsync();
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create property did not redirect to the properties list. URL='{_page.Url}'. " +
                $"Validation/alerts=[{string.Join(" | ", errors.Where(x => !string.IsNullOrWhiteSpace(x)))}]. " +
                $"Body: {body[..Math.Min(500, body.Length)]}");
        }
    }

    public Task MovePropertyDown(string listUrl, Guid propertyId) => Move(listUrl, propertyId, "down");

    public Task MovePropertyUp(string listUrl, Guid propertyId) => Move(listUrl, propertyId, "up");

    private async Task Move(string listUrl, Guid propertyId, string direction)
    {
        await _page.Navigate(listUrl);

        // Each property row has its own move-up/move-down form posting to
        // /groups/{chapterId}/properties/{id}/move/{direction}; target the one for this property.
        var button = $"form[action$='/{propertyId}/move/{direction}'] button";
        if (await _page.Locator(button).CountAsync() == 0)
        {
            throw new InvalidOperationException(
                $"No move-{direction} control for property '{propertyId}' on '{_page.Url}'.");
        }

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync(button),
            r => r.Request.Method == "POST" && r.Url.Contains($"/move/{direction}")));
    }
}
