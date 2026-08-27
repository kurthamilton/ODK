using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel member-approvals page (<c>/my/groups/{chapterId}/members/approvals</c>), which lists
/// the members waiting to be let in and carries an Approve button per row. Group Squirrel only - the route
/// is declared <c>PlatformType.Default</c> in the app - which is why this composes the path itself rather
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
    public async Task Approve(Guid chapterId, Guid memberId)
    {
        await Open(chapterId);

        // The approve form's action embeds both ids, so it locates the one row without matching on a name
        // that another test's member could share.
        var form = $"form[action='/groups/{chapterId}/members/{memberId}/approve']";

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync($"{form} button"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document"));
    }

    /// <summary>Whether the member is listed as waiting for approval.</summary>
    public async Task<bool> IsAwaitingApproval(Guid chapterId, Guid memberId)
    {
        await Open(chapterId);

        return await _page
            .Locator($"form[action='/groups/{chapterId}/members/{memberId}/approve']")
            .CountAsync() > 0;
    }

    private Task Open(Guid chapterId) =>
        _page.Navigate($"/my/groups/{chapterId}/members/approvals");
}
