using Microsoft.Playwright;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
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
    public static async Task ApproveGroup(Guid chapterId)
    {
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await RunAs(admin, page => new SiteAdminGroupsPage(page).Approve(chapterId));
    }

    public static async Task<TestGroup> CreateGroup(TestAccount owner, string name)
    {
        var chapterId = Guid.Empty;
        await RunAs(owner, async page => chapterId = await new CreateGroupPage(page).CreateGroup(name));

        var slug = await new ChapterDataHelper(E2ESettings.ConnectionString)
            .GetSlug(chapterId);
        return new TestGroup(chapterId, slug, name);
    }

    public static async Task<TestGroup> CreatePublishedGroup(TestAccount owner, string name)
    {
        var group = await CreateGroup(owner, name);
        await ApproveGroup(group.ChapterId);
        await PublishGroup(owner, group.ChapterId);
        return group;
    }

    public static async Task<TestAccount> NewAccount(string role)
    {
        var email = TestAccounts.NewEmailAddress(role);
        var password = TestAccounts.Password;

        await RunOnBrowser(page => AccountProvisioner.RegisterAndActivate(page, email, password));

        return new TestAccount(role, email, password);
    }

    public static async Task PublishGroup(TestAccount owner, Guid chapterId)
    {
        await RunAs(owner, page => new GroupAdminPage(page).Publish(chapterId));
    }

    /// <summary>
    /// Provisions a DrunkenKnitwits chapter. DrunkenKnitwits has no self-service chapter creation, so
    /// this creates a valid chapter through the Default UI (writing all dependent rows) then flips it to
    /// the DrunkenKnitwits platform and the required approval/publish state. Pass a URL-safe
    /// <paramref name="name"/> (no spaces): the DrunkenKnitwits URL segment is the chapter's ShortName,
    /// derived from the name.
    /// </summary>
    public static async Task<TestGroup> SeedDrunkenKnitwitsChapter(
        TestAccount owner, string name, bool approved = true, bool published = true)
    {
        // Joining creates the member with the platform's default site subscription, so the platform must
        // have a live payment-settings + default subscription set up.
        await EnsureDrunkenKnitwitsSubscription();

        var group = await CreateGroup(owner, name);
        await new ChapterDataHelper(E2ESettings.ConnectionString)
            .SetDrunkenKnitwitsChapter(group.ChapterId, approved, published);
        return group;
    }

    /// <summary>
    /// Ensures the DrunkenKnitwits platform has a live Stripe payment settings row and a default site
    /// subscription. The payment settings are seeded via SQL (with real Stripe keys, ready for
    /// payment-integration tests); the subscription is created through the site-admin UI (which also
    /// creates the Stripe product) and then made the platform default so <c>GetDefault</c> returns it.
    /// </summary>
    private static async Task EnsureDrunkenKnitwitsSubscription()
    {
        var payments = new SitePaymentSettingsDataHelper(E2ESettings.ConnectionString);
        var paymentSettingId = await payments.EnsureStripeSettings(
            E2ESettings.StripeApiPublicKey, E2ESettings.StripeApiSecretKey);

        if (await payments.DrunkenKnitwitsSubscriptionExists(paymentSettingId))
        {
            return;
        }

        // Create the subscription on a DrunkenKnitwits-context browser so its platform is DrunkenKnitwits
        // (the site admin logs in via the global /account/login, which DrunkenKnitwits also exposes).
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await RunAs(admin, page => new SiteAdminSubscriptionsPage(page).CreateSubscription(
            SitePaymentSettingsDataHelper.Name, "ODK E2E Free", "ODK E2E Free", groupLimit: 1, memberLimit: 20),
            E2ESettings.DrunkenKnitwitsBaseUrl);

        await new SiteSubscriptionDataHelper(E2ESettings.ConnectionString)
            .SetDrunkenKnitwitsDefault("ODK E2E Free");
    }

    private static async Task RunAs(TestAccount account, Func<IPage, Task> action, string? baseUrl = null)
    {
        await RunOnBrowser(async page =>
        {
            await new LoginPage(page).LogIn(account.Email, account.Password);
            await action(page);
        }, baseUrl);
    }

    private static async Task RunOnBrowser(Func<IPage, Task> action, string? baseUrl = null)
    {
        // Default to the Default platform (account sign-up and group creation are Default-only); callers
        // that drive a platform-specific flow pass that platform's base URL. Page objects then navigate
        // with relative paths, matching how the tests' own contexts are configured.
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = baseUrl ?? E2ESettings.DefaultBaseUrl
        });
        var page = await context.NewPageAsync();
        await action(page);
    }
}