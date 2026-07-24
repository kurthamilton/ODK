using System.Collections.Concurrent;
using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// A registry of reusable accounts keyed by role (e.g. "group-owner", "group-member"), provisioned
/// on first request and cached for the rest of the test run. The retained <see cref="TestAccount"/>
/// (email + password) lets a later test log in as an account provisioned earlier.
/// </summary>
/// <remarks>
/// Use this for accounts <em>shared</em> across tests by role. A test that needs a genuinely fresh,
/// one-off account (e.g. a member that will consume a one-time join) should mint one via
/// <see cref="Provisioning.NewAccountAsync"/> instead, so per-test state never leaks into a shared key.
/// </remarks>
internal static class SharedAccounts
{
    public const string GroupMember = "group-member";

    public const string GroupOwner = "group-owner";

    public const string SiteAdmin = "site-admin";

    // Lazy<Task<T>> memoises the async factory so each role is provisioned exactly once, even when
    // [Parallelizable] tests race to request the same key (plain GetOrAdd could run it twice).
    private static readonly ConcurrentDictionary<string, Lazy<Task<TestAccount>>> Accounts = new();

    public static Task<TestAccount> GetAsync(string role) =>
        Accounts.GetOrAdd(role, r => new Lazy<Task<TestAccount>>(() => Provisioning.NewAccountAsync(r))).Value;
}