using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel group-admin home page (<c>/my/groups/{chapterId}</c>). The Publish control is
/// only rendered once the group is approved (<c>Chapter.CanBePublished()</c>).
/// </summary>
internal class GroupAdminPage
{
    private readonly IPage _page;

    public GroupAdminPage(IPage page)
    {
        _page = page;
    }

    public async Task PublishAsync(Guid chapterId)
    {
        await _page.Navigate($"/my/groups/{chapterId}");

        await _page.ClickAsync($"form[action='/groups/{chapterId}/publish'] button");
        await _page.WaitForLoadStateAsync();
    }
}