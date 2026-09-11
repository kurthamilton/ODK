using Microsoft.Playwright;
using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel group-admin home page (<c>/my/groups/{slug}</c>). The Publish control is
/// only rendered once the group is approved and has a picture (<c>Chapter.CanBePublished()</c>); until
/// then the page offers a link to the picture page instead.
/// </summary>
internal class GroupAdminPage
{
    private readonly IPage _page;

    public GroupAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task Publish(TestGroup group)
    {
        await _page.Navigate($"/my/groups/{group.Slug}");

        await _page.ClickAndWaitForDocument($"form[action='/groups/{group.ChapterId}/publish'] button");
    }
}