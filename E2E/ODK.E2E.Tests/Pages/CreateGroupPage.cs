using System.Text.RegularExpressions;
using Microsoft.Playwright;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel create-group wizard (<c>/my/groups/new</c>): name, topics, location, picture,
/// finish. Name, location name (client-side) and lat/long + picture (server-side) are required. A new
/// group starts unapproved and unpublished.
/// </summary>
internal class CreateGroupPage
{
    // The app's feedback container. Emitted hidden and moved into place by script, so a refusal is read
    // from it rather than waited on as something visible.
    private const string FeedbackToasts = ".toasts";

    private const string SubmitButton = "[data-submit='parent']";

    private readonly IPage _page;

    public CreateGroupPage(IPage page)
    {
        _page = page;
    }

    public async Task<Guid> CreateGroup(string name)
    {
        await FillWizard(name);

        // Step 5 - finish. On success the app redirects to the new group's admin page (/my/groups/{id}).
        await _page.ClickAsync(SubmitButton);
        try
        {
            await _page.WaitForURLAsync(new Regex("/my/groups/[0-9a-fA-F-]{36}"), new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create group did not redirect to the group admin page. URL='{_page.Url}'. Page text: {body[..Math.Min(800, body.Length)]}");
        }

        return ExtractChapterId(_page.Url);
    }

    /// <summary>
    /// Drives the wizard through to submitting it, expecting the group not to be created, and returns the
    /// feedback the page came back with. A refused create renders the page again rather than redirecting, so
    /// there is no new URL to wait for - the POST response carries the answer.
    /// </summary>
    public async Task<string> CreateGroupExpectingRefusal(string name)
    {
        await FillWizard(name);

        // The POST that navigates, matching the pattern the other page objects use - the wizard's own
        // scripts post elsewhere, and returning on one of those would hand back before the submit.
        await _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync(SubmitButton),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document");

        // Text content rather than rendered text, which waits for the container to be attached rather than
        // visible and reads it either way - it is emitted hidden and moved into place by script, and the
        // rendered text of a hidden element is empty.
        return await _page.TextContentAsync(FeedbackToasts) ?? string.Empty;
    }

    private static Guid ExtractChapterId(string url)
    {
        var match = Regex.Match(
            url,
            "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not find a group id in the redirect URL '{url}'.");
        }

        return Guid.Parse(match.Value);
    }

    private async Task FillWizard(string name)
    {
        await _page.Navigate("/my/groups/new");

        // Fail fast with a useful message if the form isn't rendered (e.g. a group-limit alert or an
        // auth redirect) rather than a bare 30s "waiting for #Name" timeout.
        if (await _page.Locator("#Name").CountAsync() == 0)
        {
            var body = await _page.InnerTextAsync("body");
            throw new InvalidOperationException(
                $"Create-group form not rendered. URL='{_page.Url}'. Page text: {body[..Math.Min(800, body.Length)]}");
        }

        // Step 1 - name (required).
        await _page.FillAsync("#Name", name);
        await _page.ClickAsync("#wizard-1 .justify-content-end .btn-primary");

        // Step 2 - topics (optional).
        await _page.ClickAsync("#wizard-2 .justify-content-end .btn-primary");

        // Step 3 - location. The name is required client-side and lat/long are required server-side.
        // Set all three directly, raising only a `change` event so the Google Places autocomplete
        // (which listens on focus/input) never fires and no billable Places API call is made.
        await _page.EvalOnSelectorAsync(
            "[data-location]",
            "el => { el.value = 'London'; el.dispatchEvent(new Event('change', { bubbles: true })); }");
        await _page.EvalOnSelectorAsync("[data-location-lat]", "el => el.value = '51.5074'");
        await _page.EvalOnSelectorAsync("[data-location-long]", "el => el.value = '-0.1278'");
        await _page.ClickAsync("#wizard-3 .justify-content-end .btn-primary");

        // Step 4 - picture (required). Uploading the file triggers the Cropper.js pipeline, which
        // populates the hidden data URL asynchronously - wait for it before advancing.
        await _page.SetInputFilesAsync("[data-img-input]", TestAssets.GroupImagePath);
        await _page.WaitForFunctionAsync(
            "() => { const el = document.querySelector('[data-img-dataurl]'); return !!el && el.value.length > 0; }");
        await _page.ClickAsync("#wizard-4 .justify-content-end .btn-primary");
    }
}