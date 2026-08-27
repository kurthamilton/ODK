using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel membership settings page (<c>/my/groups/{chapterId}/members/membership</c>), where an
/// owner says whether new members need approving. Group Squirrel only, which is why this composes the path
/// itself rather than taking one from <see cref="PlatformRoutes"/>.
/// </summary>
/// <remarks>
/// The whole form sits behind the owner's MemberSubscriptions feature and the approval switch behind
/// ApproveMembers, so a group whose owner has neither renders no form at all - see
/// <c>Provisioning.EnsureMemberApprovalSiteSubscription</c>.
/// </remarks>
internal class MembershipSettingsAdminPage
{
    private const string ApproveNewMembersSwitch = "#ApproveNewMembers";

    private readonly IPage _page;

    public MembershipSettingsAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Turns on "new members need approval" and saves, leaving the group vetting whoever joins next.
    /// </summary>
    public async Task RequireApproval(Guid chapterId)
    {
        await _page.Navigate($"/my/groups/{chapterId}/members/membership");

        // Absent rather than merely unchecked when the owner's subscription does not carry the feature, so
        // say which it is - a bare timeout here looks like a slow page rather than a mis-provisioned group.
        var approve = _page.Locator(ApproveNewMembersSwitch);
        if (await approve.CountAsync() == 0)
        {
            throw new InvalidOperationException(
                $"No approval switch on the membership settings for group {chapterId}. The owner's " +
                "subscription needs the MemberSubscriptions and ApproveMembers features.");
        }

        await approve.CheckAsync();

        await _page.RunAndWaitForDocument(() => _page.RunAndWaitForResponseAsync(
            () => _page.ClickAsync("button:has-text('Update')"),
            r => r.Request.Method == "POST" && r.Request.ResourceType == "document"));
    }
}
