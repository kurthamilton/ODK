using Microsoft.Playwright;
using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel member-approvals page (<c>/my/groups/{slug}/members/approvals</c>), which lists
/// the members waiting to be let in and carries an Approve button per row. Group Squirrel only - the route
/// is declared <c>PlatformType.GroupSquirrel</c> in the app - which is why this composes the path itself rather
/// than taking one from <see cref="PlatformRoutes"/>.
/// </summary>
internal class MemberApprovalsAdminPage
{
    private readonly IPage _page;

    public MemberApprovalsAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>Approves the named member from the list, as the signed-in admin.</summary>
    public async Task Approve(TestGroup group, Guid memberId)
    {
        await Open(group);

        // The approve form's action embeds both ids, so it locates the one row without matching on a name
        // that another test's member could share.
        var form = $"form[action='/groups/{group.ChapterId}/members/{memberId}/approve']";

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync($"{form} button"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document"));
    }

    /// <summary>Whether the member is listed as waiting for approval.</summary>
    public async Task<bool> IsAwaitingApproval(TestGroup group, Guid memberId)
    {
        await Open(group);

        return await _page
            .Locator($"form[action='/groups/{group.ChapterId}/members/{memberId}/approve']")
            .CountAsync() > 0;
    }

    private Task Open(TestGroup group) =>
        _page.Navigate($"/my/groups/{group.Slug}/members/approvals");
}
