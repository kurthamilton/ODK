using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;

namespace ODK.E2E.Tests;

/// <summary>
/// Namespace-level setup/teardown for the whole E2E run.
/// <para>
/// <see cref="OneTimeSetUpAttribute"/> provisions the site admin in the <em>setup phase</em> (not as a
/// test): the account is created and activated through the UI, then promoted to site admin directly in
/// the DB (there is no self-service UI for that). The flag is read into the login claims, so it must be
/// set before the site admin logs in.
/// </para>
/// <para>
/// <see cref="OneTimeTearDownAttribute"/> runs once after every test finishes - which NUnit always does,
/// whether tests pass, fail, or error - so the data the tests created is removed regardless of outcome.
/// </para>
/// </summary>
[SetUpFixture]
public class E2ETestRunFixture
{
    /* Read before anything is provisioned, and compared against Venues.CreatedUtc to tell a venue this
       run created from one that was already here. Both timestamps come from this machine - the app under
       test runs on it - so there is no skew to allow for. */
    private static DateTime _runStartUtc;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _runStartUtc = DateTime.UtcNow;

        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new MemberAdminDataHelper(E2ESettings.ConnectionString)
            .SetSiteAdmin(admin.Email);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await Provisioning.DisposeSharedBrowser();

        var deleted = await new TestDataCleaner(E2ESettings.ConnectionString)
            .DeleteTestData(_runStartUtc);
        TestContext.Progress.WriteLine($"E2E cleanup: removed {deleted} test row(s) (members, groups, sent emails).");
    }
}