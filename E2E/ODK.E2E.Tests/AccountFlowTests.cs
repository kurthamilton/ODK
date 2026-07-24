using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

[TestFixture]
[Explicit("Requires a running Group Squirrel instance, its database, and installed Playwright browsers.")]
public class AccountFlowTests : OdkPageTest
{
    [Test]
    public async Task CreateAccount_ActivateAndLogIn_Succeeds()
    {
        // Arrange - a unique email so the test is repeatable, and a strong random password that meets
        // the password policy and won't appear in the breach check performed during activation.
        var email = TestAccounts.NewEmailAddress();
        var password = $"E2e!{Guid.NewGuid():N}Zz9";

        // Act - full journey: sign up -> activate (via the token read from the DB) -> log in.
        await AccountProvisioner.RegisterAndActivateAsync(Page, email, password);

        await new LoginPage(Page).LogInAsync(email, password);

        // Assert - the authenticated account area is reachable without being bounced back to login.
        await Page.Navigate("/account");
        Page.Url.Should().Contain("/account");
        Page.Url.Should().NotContain("/account/login");

        // Assert - registration sent the activation email (on sign-up) and the welcome email (on
        // activation) to the member, recorded in SentEmails as they passed through the email client.
        var subjects = await SentEmailDataHelper.GetSubjects(email, expectedCount: 2);
        var found = $"Subjects sent to {email}: [{string.Join(", ", subjects)}]";

        subjects.Should().Contain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            $"No activation email was sent. {found}");

        subjects.Should().Contain(
            x => x.Contains("Welcome", StringComparison.OrdinalIgnoreCase),
            $"No welcome email was sent. {found}");
    }
}