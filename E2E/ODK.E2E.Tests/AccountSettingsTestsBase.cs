using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A member's own account-management scenarios - update personal details, change email, change password -
/// written once and run against both platforms. Concrete per-platform fixtures supply the platform base
/// URL + category and provision a logged-in member with the platform-correct account routes (Default
/// account pages are global; DrunkenKnitwits are chapter-scoped, so its member belongs to a chapter).
/// </summary>
public abstract class AccountSettingsTestsBase : OdkPageTest
{
    private static EmailChangeTokenDataHelper EmailChangeTokens => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    [Test]
    public async Task ChangeEmail_NewEmailAlreadyInUse_DoesNotUpdate()
    {
        // Arrange - a logged-in member and another member whose email address is already taken.
        var (member, routes) = await ProvisionMember();
        var memberId = await Members.GetMemberId(member.Email);
        var other = await Provisioning.NewAccount("email-owner");
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        // Act - request a change to the other member's email, then follow the confirmation link.
        var emailPage = new ChangeEmailPage(Page);
        await emailPage.RequestChange(routes.ChangeEmail, other.Email);
        var token = await EmailChangeTokens.GetToken(memberId);
        await emailPage.Confirm(routes.EmailChangeConfirm(token));

        // Assert - the duplicate is rejected at confirmation; the email is unchanged.
        (await Members.GetEmailAddress(memberId)).Should().Be(member.Email);
    }

    [Test]
    public async Task ChangeEmail_WithConfirmation_UpdatesEmail()
    {
        // Arrange - a logged-in member.
        var (member, routes) = await ProvisionMember();
        var memberId = await Members.GetMemberId(member.Email);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var newEmail = TestAccounts.NewEmailAddress("changed");

        // Act - request the change, read the emailed token from the DB, then follow the confirmation link.
        var emailPage = new ChangeEmailPage(Page);
        await emailPage.RequestChange(routes.ChangeEmail, newEmail);
        var token = await EmailChangeTokens.GetToken(memberId);
        await emailPage.Confirm(routes.EmailChangeConfirm(token));

        // Assert - the member's email is now the new address.
        (await Members.GetEmailAddress(memberId)).Should().Be(newEmail);
    }

    [Test]
    public async Task ChangePassword_ValidNew_LoginWithNewPasswordWorks()
    {
        // Arrange - a logged-in member.
        var (member, routes) = await ProvisionMember();
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var newPassword = NewPassword();

        // Act - change the password using the correct current password.
        await new ChangePasswordPage(Page).Change(routes.ChangePassword, member.Password, newPassword);

        // Assert - the new password works and the old one no longer does.
        (await LoginSucceeds(member.Email, newPassword)).Should().BeTrue();
        (await LoginSucceeds(member.Email, member.Password)).Should().BeFalse();
    }

    [Test]
    public async Task ChangePassword_WrongCurrentPassword_IsRejected()
    {
        // Arrange - a logged-in member.
        var (member, routes) = await ProvisionMember();
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var attemptedPassword = NewPassword();

        // Act - attempt a change with the wrong current password.
        await new ChangePasswordPage(Page).Change(routes.ChangePassword, "WrongCurrentPassword!", attemptedPassword);

        // Assert - the change is rejected: the original password still works, the attempted one doesn't.
        (await LoginSucceeds(member.Email, member.Password)).Should().BeTrue();
        (await LoginSucceeds(member.Email, attemptedPassword)).Should().BeFalse();
    }

    [Test]
    public async Task UpdatePersonalDetails_BlankFirstName_CannotSubmit()
    {
        // Arrange - a logged-in member.
        var (member, routes) = await ProvisionMember();
        var memberId = await Members.GetMemberId(member.Email);
        var before = await Members.GetName(memberId);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        // Act - try to save with a blank first name.
        var page = new PersonalDetailsPage(Page);
        var submitted = await page.TryUpdate(routes.PersonalDetails, string.Empty, "Someone");

        // Assert - the form is blocked, the field shows an error, and the name is unchanged.
        submitted.Should().BeFalse();
        (await page.FieldErrorShown("FirstName")).Should().BeTrue();
        (await Members.GetName(memberId)).Should().Be(before);
    }

    [Test]
    public async Task UpdatePersonalDetails_ValidNames_ArePersisted()
    {
        // Arrange - a logged-in member.
        var (member, routes) = await ProvisionMember();
        var memberId = await Members.GetMemberId(member.Email);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var firstName = $"First{Guid.NewGuid():N}"[..12];
        var lastName = $"Last{Guid.NewGuid():N}"[..12];

        // Act - update the name.
        var submitted = await new PersonalDetailsPage(Page).TryUpdate(routes.PersonalDetails, firstName, lastName);
        submitted.Should().BeTrue();

        // Assert - the new name is persisted.
        (await Members.GetName(memberId)).Should().Be((firstName, lastName));
    }

    protected abstract Task<(TestAccount Member, AccountRoutes Routes)> ProvisionMember();

    private static string NewPassword() => $"E2eNew!{Guid.NewGuid():N}Zz9";

    // Verifies credentials on a fresh, unauthenticated context (the test's own context is already logged
    // in as the member, so a password change can only be proved by a clean login).
    private async Task<bool> LoginSucceeds(string email, string password)
    {
        await using var context = await Browser.NewContextAsync(ContextOptions());
        var page = await context.NewPageAsync();
        return await new LoginPage(page).TryLogIn(email, password);
    }
}