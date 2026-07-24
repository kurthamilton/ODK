using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The site-admin groups page (<c>/siteadmin/groups</c>), where a site admin approves pending groups.
/// Requires the logged-in member to be a site admin (<c>Members.SuperAdmin = 1</c>).
/// </summary>
internal class SiteAdminGroupsPage
{
    private readonly IPage _page;

    public SiteAdminGroupsPage(IPage page)
    {
        _page = page;
    }

    public async Task ApproveAsync(Guid chapterId)
    {
        await _page.Navigate("/siteadmin/groups");

        // Pending groups live in the (initially hidden) Pending tab pane - activate it first so the
        // approve button is visible.
        await _page.ClickAsync("button[data-bs-target='#pending-tab-pane']");

        // The approve form's action embeds the group id, so it's a unique, stable locator.
        await _page.ClickAsync($"form[action='/siteadmin/groups/{chapterId}/approve'] button");
        await _page.WaitForLoadStateAsync();
    }
}