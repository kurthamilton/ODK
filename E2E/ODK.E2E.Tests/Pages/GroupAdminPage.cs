using Microsoft.Playwright;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// The Group Squirrel group-admin home page (<c>/my/groups/{slug}</c>), which carries the setup
/// checklist. The Publish control is only rendered once the group is approved and has a picture
/// (<c>Chapter.CanBePublished()</c>); until then the page offers a link to the picture page instead, and
/// Submit for approval is likewise replaced by the reason it cannot be used yet.
/// </summary>
internal class GroupAdminPage
{
    private readonly IPage _page;

    public GroupAdminPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Whether the checklist is offering submission rather than saying what is holding it up. Both are
    /// rendered, so this reads which - a presence check on the checklist itself would pass either way.
    /// </summary>
    public async Task<bool> CanSubmitForApproval(TestGroup group)
    {
        await Open(group);

        return await _page.Locator(SubmitForm(group) + " button").CountAsync() > 0;
    }

    /// <summary>
    /// Skips an optional checklist step. The step is named by the form's action rather than by the button,
    /// which is a bare icon every dismissable step shares.
    /// </summary>
    public async Task DismissChecklistItem(TestGroup group, int checklistItemTypeId)
    {
        await Open(group);

        await _page.RunAndWaitForDocument(async () =>
        {
            await _page.ClickAsync(DismissForm(group, checklistItemTypeId) + " button");
            await _page.AcceptConfirm();
        });
    }

    public Task Open(TestGroup group) => _page.Navigate($"/my/groups/{group.Slug}");

    public async Task Publish(TestGroup group)
    {
        await Open(group);

        await _page.ClickAndWaitForDocument($"form[action='/groups/{group.ChapterId}/publish'] button");
    }

    public async Task SubmitForApproval(TestGroup group)
    {
        await Open(group);

        await _page.ClickAndWaitForDocument(SubmitForm(group) + " button");
    }

    private static string DismissForm(TestGroup group, int checklistItemTypeId)
        => $"form[action='/groups/{group.ChapterId}/checklist/"
           + $"{ChecklistItemTypeIds.Name(checklistItemTypeId)}/dismiss']";

    private static string SubmitForm(TestGroup group)
        => $"form[action='/groups/{group.ChapterId}/submit']";
}
