using Microsoft.Playwright;
using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel group texts page (<c>/my/groups/{slug}/texts</c>): the group's description and the
/// messages new members see. Group Squirrel only, which is why this composes the path itself rather than
/// taking one from <see cref="PlatformRoutes"/>.
/// </summary>
/// <remarks>
/// Filling this is what completes the Description step of the setup checklist, which reads the short
/// description and the description together - the other two fields are required by the form rather than
/// by the checklist, and are filled because the form will not save without them.
/// </remarks>
internal class GroupTextsAdminPage
{
    private const string ShortDescriptionField = "#ShortDescription";

    private readonly IPage _page;

    public GroupTextsAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task Describe(TestGroup group, string shortDescription, string description)
    {
        await _page.Navigate($"/my/groups/{group.Slug}/texts");

        // Plain text rather than rich text, and only rendered on a platform that shows short descriptions.
        await _page.FillAsync(ShortDescriptionField, shortDescription);

        await _page.SetHtmlEditor("DescriptionHtml", $"<p>{description}</p>");
        await _page.SetHtmlEditor("RegisterMessageHtml", $"<p>{description}</p>");
        await _page.SetHtmlEditor("WelcomeMessageHtml", $"<p>{description}</p>");

        /* The POST that navigates. Any-POST would also match the editors' own HTML check, which posts the
           content to the validate endpoint whenever it changes, and returning on that hands back before
           the save has happened. */
        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Update')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document"));
    }
}
