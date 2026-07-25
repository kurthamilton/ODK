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
    // Set up the DrunkenKnitwits Stripe product / default subscription exactly once per run, even when
    // parallel DrunkenKnitwits fixtures race to seed it (Lazy runs the factory a single time).
    private static readonly Lazy<Task> DrunkenKnitwitsSubscription = new(EnsureDrunkenKnitwitsSubscriptionOnce);

    // One shared Playwright driver + browser for ALL provisioning, launched once. Each provisioning call
    // gets a fresh, isolated context (its own cookies/login), which is cheap - so we avoid re-spawning the
    // driver and re-launching a browser on every account/group/member we set up. Disposed at the end of
    // the run via DisposeSharedBrowser. (The tests' own browsers are separate, pooled by Playwright.NUnit.)
    private static readonly Lazy<Task<IBrowser>> SharedBrowser = new(LaunchSharedBrowser);

    private static IPlaywright? _playwright;

    public static async Task ApproveGroup(Guid chapterId)
    {
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await RunAs(admin, page => new SiteAdminGroupsPage(page).Approve(chapterId));
    }

    /// <summary>
    /// Disposes the shared provisioning browser and Playwright driver. Call once after the whole run.
    /// </summary>
    public static async Task DisposeSharedBrowser()
    {
        if (SharedBrowser.IsValueCreated)
        {
            var browser = await SharedBrowser.Value;
            await browser.DisposeAsync();
        }

        _playwright?.Dispose();
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

    /// <summary>
    /// Creates a draft (unpublished) event (with its own venue) as the group owner, through the
    /// platform's admin UI, and returns its id + shortcode. Used by tests that check draft visibility.
    /// </summary>
    public static Task<TestEvent> CreateDraftEvent(
        TestAccount owner, PlatformRoutes routes, Guid chapterId, string baseUrl)
        => CreateEvent(owner, routes, chapterId, baseUrl, draft: true);

    /// <summary>
    /// Creates a published event (with its own venue) as the group owner, through the platform's admin
    /// UI, and returns its id + shortcode. Used by RSVP/listing tests that need an event. Drives the
    /// owner on a throwaway browser against <paramref name="baseUrl"/> (the platform's port), with
    /// <paramref name="routes"/> supplying the platform-correct admin URLs. Pass
    /// <paramref name="attendeeLimit"/> to cap the number of attendees.
    /// </summary>
    public static Task<TestEvent> CreatePublishedEvent(
        TestAccount owner, PlatformRoutes routes, Guid chapterId, string baseUrl, int? attendeeLimit = null)
        => CreateEvent(owner, routes, chapterId, baseUrl, draft: false, attendeeLimit);

    /// <summary>
    /// Provisions a fresh member of a Default group: a new account joins through the UI. The member is
    /// approved automatically by the platform (the chapter's subscription has no ApproveMembers feature),
    /// so this doesn't touch the approval state itself.
    /// </summary>
    public static async Task<TestAccount> JoinGroupAsMember(TestGroup group)
    {
        var member = await NewAccount(SharedAccounts.GroupMember);
        await RunAs(member, page => new JoinGroupPage(page).Join(group.Slug));
        return member;
    }

    /// <summary>
    /// Provisions a fresh member of a DrunkenKnitwits chapter. On DrunkenKnitwits joining the chapter IS
    /// the sign-up, so this runs the join (= register) + activate flow against the chapter. The member is
    /// approved automatically by the platform (the chapter's subscription has no ApproveMembers feature).
    /// </summary>
    public static async Task<TestAccount> JoinDrunkenKnitwitsChapterAsMember(TestGroup group)
    {
        var email = TestAccounts.NewEmailAddress(SharedAccounts.GroupMember);
        var password = TestAccounts.Password;
        var shortName = group.Name.ToLowerInvariant();

        await RunOnBrowser(async page =>
        {
            await new DrunkenKnitwitsJoinPage(page).Join(shortName, "E2E", "Test", email);

            var token = await new ActivationTokenDataHelper(E2ESettings.ConnectionString)
                .GetActivationToken(email);

            await new DrunkenKnitwitsActivatePage(page).Activate(shortName, token, password);
        }, E2ESettings.DrunkenKnitwitsBaseUrl);

        return new TestAccount(SharedAccounts.GroupMember, email, password);
    }

    /// <summary>
    /// Creates a chapter member-profile property (question) through the admin UI, as the group owner, and
    /// returns its id. Pass <paramref name="required"/> / <paramref name="applicationOnly"/> to set those
    /// flags.
    /// </summary>
    public static async Task<Guid> CreateChapterProperty(
        TestAccount owner, PlatformRoutes routes, Guid chapterId, string baseUrl,
        string label, bool required = false, bool applicationOnly = false)
    {
        await RunAs(owner, page => new ChapterPropertyAdminPage(page)
            .CreateProperty(routes.PropertyCreate, label, required, applicationOnly), baseUrl);

        return await new ChapterPropertyDataHelper(E2ESettings.ConnectionString).GetPropertyId(chapterId, label)
            ?? throw new InvalidOperationException($"Chapter property '{label}' was not created.");
    }

    /// <summary>
    /// Provisions a fresh member of a DrunkenKnitwits chapter who answers the given chapter properties as
    /// part of sign-up (= join), then activates. Throws if the join is blocked.
    /// </summary>
    public static async Task<TestAccount> JoinDrunkenKnitwitsMemberWithProperties(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
    {
        var email = TestAccounts.NewEmailAddress(SharedAccounts.GroupMember);
        var password = TestAccounts.Password;
        var shortName = group.Name.ToLowerInvariant();

        var joined = false;
        await RunOnBrowser(async page =>
        {
            joined = await new DrunkenKnitwitsJoinPage(page)
                .TryJoinWithProperties(shortName, "E2E", "Test", email, answers);
            if (!joined)
            {
                return;
            }

            var token = await new ActivationTokenDataHelper(E2ESettings.ConnectionString)
                .GetActivationToken(email);
            await new DrunkenKnitwitsActivatePage(page).Activate(shortName, token, password);
        }, E2ESettings.DrunkenKnitwitsBaseUrl);

        if (!joined)
        {
            throw new InvalidOperationException($"Member '{email}' could not sign up with the given properties.");
        }

        return new TestAccount(SharedAccounts.GroupMember, email, password);
    }

    /// <summary>
    /// Provisions a fresh member of a Default group who answers the given chapter properties when joining.
    /// Throws if the join is blocked.
    /// </summary>
    public static async Task<TestAccount> JoinGroupMemberWithProperties(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
    {
        var member = await NewAccount(SharedAccounts.GroupMember);

        var joined = false;
        await RunAs(member, async page =>
            joined = await new JoinGroupPage(page).TryJoinWithProperties(group.Slug, answers));

        if (!joined)
        {
            throw new InvalidOperationException($"Member '{member.Email}' could not join with the given properties.");
        }

        return member;
    }

    /// <summary>Moves a chapter property one place down (later) in display order, via the admin UI.</summary>
    public static Task MoveChapterPropertyDown(
        TestAccount owner, PlatformRoutes routes, Guid propertyId, string baseUrl)
        => RunAs(owner, page => new ChapterPropertyAdminPage(page)
            .MovePropertyDown(routes.PropertiesList, propertyId), baseUrl);

    public static async Task<TestAccount> NewAccount(string role)
    {
        var email = TestAccounts.NewEmailAddress(role);
        var password = TestAccounts.Password;

        await RunOnBrowser(page => AccountProvisioner.RegisterAndActivate(page, email, password));

        return new TestAccount(role, email, password);
    }

    /// <summary>
    /// Attempts a DrunkenKnitwits sign-up answering only the given (partial) properties; returns whether
    /// it succeeded. Used to assert a join is blocked when a required property is left blank.
    /// </summary>
    public static async Task<bool> TryJoinDrunkenKnitwitsWithoutRequired(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
    {
        var email = TestAccounts.NewEmailAddress(SharedAccounts.GroupMember);
        var shortName = group.Name.ToLowerInvariant();

        var joined = false;
        await RunOnBrowser(async page =>
            joined = await new DrunkenKnitwitsJoinPage(page)
                .TryJoinWithProperties(shortName, "E2E", "Test", email, answers),
            E2ESettings.DrunkenKnitwitsBaseUrl);

        return joined;
    }

    /// <summary>
    /// Attempts a Default group join answering only the given (partial) properties; returns whether it
    /// succeeded. Used to assert a join is blocked when a required property is left blank.
    /// </summary>
    public static async Task<bool> TryJoinGroupWithoutRequired(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers)
    {
        var member = await NewAccount(SharedAccounts.GroupMember);

        var joined = false;
        await RunAs(member, async page =>
            joined = await new JoinGroupPage(page).TryJoinWithProperties(group.Slug, answers));

        return joined;
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
    private static async Task<TestEvent> CreateEvent(
        TestAccount owner, PlatformRoutes routes, Guid chapterId, string baseUrl, bool draft, int? attendeeLimit = null)
    {
        var venueName = $"E2E Venue {Guid.NewGuid():N}";
        var eventName = $"E2E Event {Guid.NewGuid():N}";
        var date = $"{DateTime.Today.AddDays(14):dd/MM/yyyy} 19:00";

        await RunAs(owner, async page =>
        {
            await new VenueAdminPage(page).CreateVenue(routes.VenueCreate, venueName);

            var venueId = await new VenueDataHelper(E2ESettings.ConnectionString).GetVenueId(chapterId, venueName)
                ?? throw new InvalidOperationException($"Venue '{venueName}' was not created.");

            await new EventAdminPage(page).CreateEvent(routes.EventCreate, eventName, venueId, date, draft, attendeeLimit);
        }, baseUrl);

        var events = new EventDataHelper(E2ESettings.ConnectionString);
        var eventId = await events.GetEventId(chapterId, eventName)
            ?? throw new InvalidOperationException($"Event '{eventName}' was not created.");
        var shortcode = await events.GetShortcode(eventId);

        return new TestEvent(eventId, eventName, shortcode);
    }

    private static Task EnsureDrunkenKnitwitsSubscription() => DrunkenKnitwitsSubscription.Value;

    private static async Task EnsureDrunkenKnitwitsSubscriptionOnce()
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
        // Fresh, isolated context off the shared browser. Default to the Default platform (account
        // sign-up and group creation are Default-only); callers driving a platform-specific flow pass that
        // platform's base URL. Page objects then navigate with relative paths, matching the tests' own
        // contexts.
        var browser = await SharedBrowser.Value;
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = baseUrl ?? E2ESettings.DefaultBaseUrl
        });
        var page = await context.NewPageAsync();
        await action(page);
    }

    private static async Task<IBrowser> LaunchSharedBrowser()
    {
        _playwright = await Playwright.CreateAsync();
        return await _playwright.Chromium.LaunchAsync();
    }
}