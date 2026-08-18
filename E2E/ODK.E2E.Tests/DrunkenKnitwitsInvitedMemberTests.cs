using System.Web;
using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// An imported member's journey into a DrunkenKnitwits chapter. An import records an <em>invitation</em> and
/// no membership, so membership begins when the member accepts - and because signing up on DrunkenKnitwits
/// is joining, accepting happens on the chapter's own join page.
/// </summary>
/// <remarks>
/// <para>
/// Each test provisions its own chapter. Importing writes membership and approval state to the group, which
/// is exactly the dynamic, multi-actor state the isolation rules say must be local rather than shared.
/// </para>
/// <para>
/// Two things are read from the database because a browser cannot see them: the invitation's token (the
/// emailed link carries it, and <c>SentEmails</c> records only subjects, never bodies) and the resulting
/// membership. That is the same compromise <c>ActivationTokenDataHelper</c> already makes.
/// </para>
/// </remarks>
[TestFixture]
[Category("AccountWorkflows")]
[Category("ChapterMembershipWorkflows")]
public class DrunkenKnitwitsInvitedMemberTests : DrunkenKnitwitsPageTest
{
    /// <summary>
    /// Mirrors the subject the <c>MemberImportInvite</c> template is seeded with. Held here so a wording
    /// change is a one-line update rather than a hunt through assertions.
    /// </summary>
    private const string InviteSubjectFragment = "invited to join";

    [Test]
    public async Task ImportMembers_NewMember_InvitesThemWithoutMembershipOrAnActivationEmail()
    {
        // Arrange
        var (group, owner) = await SeedChapter();
        var email = TestAccounts.NewEmailAddress();

        // Act
        await Import(owner, group, email);

        // Assert - invited, but not yet a member: membership waits for them to accept.
        (await MemberChapterInviteDataHelper.HasInvite(email, group.ChapterId)).Should().BeTrue(
            "the import records an invitation");
        (await ChapterDataHelper.IsMember(email, group.ChapterId)).Should().BeFalse(
            "an imported member has no membership status until they accept");

        /* Assert - the invitation, not an activation link. On this platform the invitation's link lands on
           the pre-filled join page, where the account is created; an activation link would take them
           straight past it into an account belonging to no group. */
        var subjects = await SentEmailDataHelper.GetSubjects(email, expectedCount: 1);
        var found = $"Subjects sent to {email}: [{string.Join(", ", subjects)}]";

        subjects.Should().ContainSingle(found);
        subjects.Should().Contain(
            x => x.Contains(InviteSubjectFragment, StringComparison.OrdinalIgnoreCase),
            $"No invitation email was sent. {found}");
        subjects.Should().NotContain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            $"An activation email was sent instead of the invitation. {found}");
    }

    [Test]
    public async Task AcceptInvitation_KeepingTheInvitedAddress_GoesStraightToSettingAPassword()
    {
        // Arrange - an imported member following the link they were emailed.
        var (group, owner) = await SeedChapter();
        var email = TestAccounts.NewEmailAddress();
        await Import(owner, group, email);

        var inviteToken = await MemberChapterInviteDataHelper.GetInviteToken(email, group.ChapterId);
        var shortName = ShortName(group);

        var joinPage = new DrunkenKnitwitsJoinPage(Page);
        await joinPage.OpenInvitation(shortName, inviteToken);

        // Assert - the form arrives filled in with what the group already holds about them.
        (await joinPage.GetEmailAddress()).Should().Be(email);
        (await joinPage.GetFirstName()).Should().Be("Imported");
        (await joinPage.GetLastName()).Should().Be("Member");

        // Act - submit with the address the invitation was sent to.
        var landedOn = await joinPage.AcceptInvitation();

        /* Assert - straight to setting a password. Holding the token proves they read mail at that address,
           which is all an activation email establishes, so the URL carries the activation token instead of
           an email doing it. */
        landedOn.Should().ContainEquivalentOf("/account/activate");

        var activationToken = await ActivationTokenDataHelper.GetActivationToken(email);
        TokenFrom(landedOn).Should().Be(activationToken);

        var subjectsBeforeActivating = await SentEmailDataHelper.GetSubjects(email, expectedCount: 1);
        subjectsBeforeActivating.Should().NotContain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            "signing up with the invited address needs no activation email: " +
            $"[{string.Join(", ", subjectsBeforeActivating)}]");

        // Act - set the password the page is asking for, then sign in with it.
        var password = TestAccounts.Password;
        await new DrunkenKnitwitsActivatePage(Page).Activate(shortName, activationToken, password);
        await new DrunkenKnitwitsLoginPage(Page).LogIn(shortName, email, password);

        // Assert - a member of the group, and the invitation is consumed rather than left outstanding.
        Page.Url.Should().NotContainEquivalentOf("/account/login");
        (await ChapterDataHelper.IsMember(email, group.ChapterId)).Should().BeTrue();
        (await MemberChapterInviteDataHelper.HasInvite(email, group.ChapterId)).Should().BeFalse();
    }

    [Test]
    public async Task AcceptInvitation_ChangingTheEmailAddress_FallsBackToAnActivationEmail()
    {
        /* Arrange - the same link, but the member corrects the address the import supplied. The token says
           nothing about an address it was not sent to, so the new one has to be proved the usual way. */
        var (group, owner) = await SeedChapter();
        var invitedEmail = TestAccounts.NewEmailAddress();
        await Import(owner, group, invitedEmail);

        var inviteToken = await MemberChapterInviteDataHelper.GetInviteToken(invitedEmail, group.ChapterId);
        var correctedEmail = TestAccounts.NewEmailAddress();

        var joinPage = new DrunkenKnitwitsJoinPage(Page);
        await joinPage.OpenInvitation(ShortName(group), inviteToken);

        // Act
        var landedOn = await joinPage.AcceptInvitation(replacementEmailAddress: correctedEmail);

        // Assert - the ordinary "check your email" path, and the email goes to the address they typed.
        landedOn.Should().ContainEquivalentOf("/account/pending");

        var subjects = await SentEmailDataHelper.GetSubjects(correctedEmail, expectedCount: 1);
        subjects.Should().Contain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            $"No activation email was sent. Subjects sent to {correctedEmail}: [{string.Join(", ", subjects)}]");
    }

    [Test]
    public async Task AcceptInvitation_MemberWhoAlreadyHasAnAccount_SignsInAndJoins()
    {
        // Arrange - somebody with an account already, imported into a group they are not in.
        var (group, owner) = await SeedChapter();
        var member = await Provisioning.NewAccount("dk-invited-member");
        await Import(owner, group, member.Email);

        var inviteToken = await MemberChapterInviteDataHelper.GetInviteToken(member.Email, group.ChapterId);
        var shortName = ShortName(group);

        var joinPage = new DrunkenKnitwitsJoinPage(Page);
        await joinPage.OpenInvitation(shortName, inviteToken);

        /* Assert - no sign-up form. Offering one could only tell them the address is taken and leave the
           invitation outstanding, so the page asks them to sign in. */
        (await joinPage.HasSignUpForm()).Should().BeFalse();
        (await joinPage.HasSignInPrompt()).Should().BeTrue();

        // Act - sign in from the prompt, whose return URL brings them back to the invitation.
        await joinPage.FollowSignInPrompt();
        await new DrunkenKnitwitsLoginPage(Page).LogInOnCurrentPage(member.Email, member.Password);

        Page.Url.Should().ContainEquivalentOf("/account/join");

        await joinPage.JoinAsSignedInMember(shortName);

        // Assert - joined, and the invitation is consumed.
        (await ChapterDataHelper.IsMember(member.Email, group.ChapterId)).Should().BeTrue();
        (await MemberChapterInviteDataHelper.HasInvite(member.Email, group.ChapterId)).Should().BeFalse();
    }

    /// <summary>
    /// A chapter of its own per test: importing writes membership and approval state to the group, which the
    /// isolation rules keep local rather than shared.
    /// </summary>
    private static async Task<(TestGroup Group, TestAccount Owner)> SeedChapter()
    {
        var owner = await Provisioning.NewAccount("dk-invite-owner");
        var group = await Provisioning.SeedDrunkenKnitwitsChapter(owner, $"e2edk{Guid.NewGuid():N}");
        return (group, owner);
    }

    // The DrunkenKnitwits URL segment is the chapter's ShortName - the un-suffixed name, lowercased.
    private static string ShortName(TestGroup group) => group.Name.ToLowerInvariant();

    private static string TokenFrom(string url)
        => HttpUtility.ParseQueryString(new Uri(url).Query)["token"] ?? string.Empty;

    private Task Import(TestAccount owner, TestGroup group, string emailAddress) =>
        Provisioning.ImportMembers(
            owner,
            PlatformRoutes.DrunkenKnitwits(group),
            PlatformBaseUrl,
            [new MemberImportRow
            {
                EmailAddress = emailAddress,
                FirstName = "Imported",
                LastName = "Member"
            }]);
}
