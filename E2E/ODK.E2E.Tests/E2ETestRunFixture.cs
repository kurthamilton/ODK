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
    [OneTimeSetUp]
    public async Task SetUp()
    {
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new MemberAdminDataHelper(E2ESettings.ConnectionString)
            .SetSiteAdmin(admin.Email);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        var deleted = await new TestDataCleaner(E2ESettings.ConnectionString)
            .DeleteTestData();
        TestContext.Progress.WriteLine($"E2E cleanup: removed {deleted} test row(s) (members, groups, sent emails).");

        // Leave the seeded Stripe payment settings in place but deactivated (Active = 0) so they don't
        // affect normal app behaviour between runs.
        await new SitePaymentSettingsDataHelper(E2ESettings.ConnectionString).Deactivate();
    }
}