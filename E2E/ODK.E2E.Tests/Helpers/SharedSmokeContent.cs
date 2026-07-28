using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Run-once shared context for the member-facing page smoke tests (see <see cref="SmokeContext"/>): one
/// published chapter per platform, each with a joined member and a published event, provisioned on first
/// request (thread-safe <see cref="Lazy{T}"/>) and reused read-only. Dedicated to the smoke suite so
/// adding the event/member never touches another test's shared chapter. Cleaned by the namespace
/// teardown with all other test data (the owner uses the test email domain).
/// </summary>
internal static class SharedSmokeContent
{
    private static readonly Lazy<Task<SmokeContext>> DefaultContext = new(ProvisionDefault);

    private static readonly Lazy<Task<SmokeContext>> DrunkenKnitwitsContext = new(ProvisionDrunkenKnitwits);

    public static Task<SmokeContext> Default() => DefaultContext.Value;

    public static Task<SmokeContext> DrunkenKnitwits() => DrunkenKnitwitsContext.Value;

    private static async Task<SmokeContext> ProvisionDefault()
    {
        var owner = await Provisioning.NewAccount("smoke-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, Name());
        var routes = PlatformRoutes.Default(group);
        // The profile page only renders its form once the chapter has a property; add one (optional, so a
        // plain member join isn't blocked) so the profile smoke has something to assert on.
        await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, E2ESettings.DefaultBaseUrl, $"E2E smoke {Guid.NewGuid():N}");
        var @event = await Provisioning.CreatePublishedEvent(
            owner, routes, group.ChapterId, E2ESettings.DefaultBaseUrl);
        var member = await Provisioning.JoinGroupAsMember(group);
        return new SmokeContext(group, member, @event);
    }

    private static async Task<SmokeContext> ProvisionDrunkenKnitwits()
    {
        var owner = await Provisioning.NewAccount("smoke-owner");
        var group = await Provisioning.SeedDrunkenKnitwitsChapter(owner, Name());
        var @event = await Provisioning.CreatePublishedEvent(
            owner, PlatformRoutes.DrunkenKnitwits(group), group.ChapterId, E2ESettings.DrunkenKnitwitsBaseUrl);
        var member = await Provisioning.JoinDrunkenKnitwitsChapterAsMember(group);
        return new SmokeContext(group, member, @event);
    }

    // The chapter name must be letters-only: the DrunkenKnitwits home route (/{chapterName}) is
    // constrained to ^[A-Za-z-]+$, so a name with digits wouldn't match it. Map each GUID digit to a
    // distinct letter (k-t), leaving the hex letters a-f untouched - the two ranges don't overlap, so
    // names stay unique and letters-only.
    private static string Name()
        => "esmoke" + new string(Guid.NewGuid().ToString("N")
            .Select(c => c <= '9' ? (char)('k' + (c - '0')) : c).ToArray());
}
