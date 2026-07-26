using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// A run-once registry of published chapters shared across tests as read-only CONTEXT (see the
/// "Test isolation: shared vs local provisioning" section in CLAUDE.md). Each chapter is provisioned
/// exactly once (thread-safe <see cref="Lazy{T}"/>) and reused, so a test that only needs "a published
/// chapter of type X" - to join a fresh member to, or add uniquely-named data to - doesn't re-run the
/// expensive create-chapter flow every time.
/// <para>
/// STRICT RULES for callers: never mutate a shared chapter's own state (event settings, its property
/// set/order, anything a test asserts a count/order/emptiness of) and never rely on it being "clean" -
/// other tests may have joined members or added uniquely-named data. Anything that mutates or asserts the
/// chapter's aggregate/dynamic state (attendance limits, reordering, membership counts) MUST use a local
/// chapter from <see cref="Provisioning"/> instead. When in doubt, go local.
/// </para>
/// <para>
/// Capacity note: a shared chapter's member headroom is bounded by its owner subscription's member limit
/// (the DrunkenKnitwits "ODK E2E Free" seed is 20). If enough tests share one chapter to approach that,
/// raise the seeded limit rather than silently letting joins fail. Cleaned by the namespace teardown with
/// all other test data (the owner uses the test email domain).
/// </para>
/// </summary>
internal static class SharedChapters
{
    // Free-subscription published chapters, one per platform, provisioned on first request. Key by
    // subscription "type" here too if a differently-provisioned shared chapter is ever needed.
    private static readonly Lazy<Task<TestGroup>> DefaultFree = new(ProvisionDefault);

    private static readonly Lazy<Task<TestGroup>> DrunkenKnitwitsFree = new(ProvisionDrunkenKnitwits);

    /// <summary>A shared published Default (Group Squirrel) chapter on the free subscription.</summary>
    public static Task<TestGroup> Default() => DefaultFree.Value;

    /// <summary>A shared published DrunkenKnitwits chapter on the free subscription.</summary>
    public static Task<TestGroup> DrunkenKnitwits() => DrunkenKnitwitsFree.Value;

    private static async Task<TestGroup> ProvisionDefault()
    {
        var owner = await Provisioning.NewAccount("shared-chapter-owner");
        return await Provisioning.CreatePublishedGroup(owner, Name());
    }

    private static async Task<TestGroup> ProvisionDrunkenKnitwits()
    {
        var owner = await Provisioning.NewAccount("shared-chapter-owner");
        return await Provisioning.SeedDrunkenKnitwitsChapter(owner, Name());
    }

    private static string Name() => $"e2eshared{Guid.NewGuid():N}";
}
