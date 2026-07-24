using Microsoft.Playwright;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Provisions the foundations tests build on - accounts and groups in a required state - by driving
/// the real UI as the relevant actor on a throwaway browser (independent of a test's own PageTest
/// browser). Group creation is expensive (a five-step wizard with a file upload), so tests provision
/// only what they need and reuse shared accounts via <see cref="SharedAccounts"/> where possible.
/// </summary>
internal static class Provisioning
{
    public static async Task ApproveGroupAsync(Guid chapterId)
    {
        var admin = await SharedAccounts.GetAsync(SharedAccounts.SiteAdmin);
        await RunAsAsync(admin, page => new SiteAdminGroupsPage(page).ApproveAsync(chapterId));
    }

    public static async Task<TestGroup> CreateGroupAsync(TestAccount owner, string name)
    {
        var chapterId = Guid.Empty;
        await RunAsAsync(owner, async page => chapterId = await new CreateGroupPage(page).CreateGroupAsync(name));

        var slug = await ChapterDataHelper.GetSlug(chapterId);
        return new TestGroup(chapterId, slug, name);
    }

    public static async Task<TestGroup> CreatePublishedGroupAsync(TestAccount owner, string name)
    {
        var group = await CreateGroupAsync(owner, name);
        await ApproveGroupAsync(group.ChapterId);
        await PublishGroupAsync(owner, group.ChapterId);
        return group;
    }

    public static async Task<TestAccount> NewAccountAsync(string role)
    {
        var email = TestAccounts.NewEmailAddress(role);
        var password = TestAccounts.Password;

        await RunOnBrowserAsync(page => AccountProvisioner.RegisterAndActivateAsync(page, email, password));

        return new TestAccount(role, email, password);
    }

    public static async Task PublishGroupAsync(TestAccount owner, Guid chapterId)
    {
        await RunAsAsync(owner, page => new GroupAdminPage(page).PublishAsync(chapterId));
    }

    private static async Task RunAsAsync(TestAccount account, Func<IPage, Task> action)
    {
        await RunOnBrowserAsync(async page =>
        {
            await new LoginPage(page).LogInAsync(account.Email, account.Password);
            await action(page);
        });
    }

    private static async Task RunOnBrowserAsync(Func<IPage, Task> action)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await action(page);
    }
}