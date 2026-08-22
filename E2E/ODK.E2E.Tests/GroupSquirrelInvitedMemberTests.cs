using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// An imported member's journey into a Group Squirrel group. An import records an <em>invitation</em> and no
/// membership, so membership begins when the member accepts - and because a member here exists before any
/// group does, accepting happens on a page of its own that gives the account its first password and joins the
/// group in the same submit.
/// </summary>
/// <remarks>
/// <para>
/// Each test provisions its own group. Importing writes membership and invitation state to it, which is
/// exactly the dynamic, multi-actor state the isolation rules say must be local rather than shared.
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
public class GroupSquirrelInvitedMemberTests : DefaultPageTest
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
        var (group, owner) = await SeedGroup();
        var email = TestAccounts.NewEmailAddress();

        // Act
        await Import(owner, group, email);

        // Assert - invited, but not yet a member: membership waits for them to accept.
        (await MemberChapterInviteDataHelper.HasInvite(email, group.ChapterId)).Should().BeTrue(
            "the import records an invitation");
        (await ChapterDataHelper.IsMember(email, group.ChapterId)).Should().BeFalse(
            "an imported member has no membership status until they accept");

        /* Assert - the invitation, not an activation link. The invitation's link lands on a page that gives
           the account its first password and joins the group; an activation link would take them straight
           past it into an account belonging to no group. */
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
    public async Task AcceptInvitation_NewMember_SetsAPasswordAndJoinsInOneGo()
    {
        // Arrange - an imported member following the link they were emailed.
        var (group, owner) = await SeedGroup();
        var email = TestAccounts.NewEmailAddress();
        await Import(owner, group, email);

        var inviteToken = await MemberChapterInviteDataHelper.GetInviteToken(email, group.ChapterId);

        var acceptPage = new AcceptInvitePage(Page);
        await acceptPage.Open(group.Slug, inviteToken);

        // Assert - the form arrives filled in with what the group already holds about them.
        (await acceptPage.GetEmailAddress()).Should().Be(email);
        (await acceptPage.GetFirstName()).Should().Be("Imported");
        (await acceptPage.GetLastName()).Should().Be("Member");

        // Act
        var password = TestAccounts.Password;
        var landedOn = await acceptPage.Accept(password);

        /* Assert - the account and the membership are both done by that one submit, so all that is left is
           signing in. No activation email either: holding the token proves they read mail at that address,
           which is all an activation email establishes. */
        landedOn.Should().ContainEquivalentOf("/account/login");

        (await ChapterDataHelper.IsMember(email, group.ChapterId)).Should().BeTrue();
        (await MemberChapterInviteDataHelper.HasInvite(email, group.ChapterId)).Should().BeFalse(
            "accepting consumes the invitation rather than leaving them permanently invited");

        var subjects = await SentEmailDataHelper.GetSubjects(email, expectedCount: 1);
        subjects.Should().NotContain(
            x => x.Contains("Activate", StringComparison.OrdinalIgnoreCase),
            $"accepting needs no activation email: [{string.Join(", ", subjects)}]");

        // Act + assert - the password they just set is the one that signs them in.
        await new LoginPage(Page).LogIn(email, password);
        Page.Url.Should().NotContainEquivalentOf("/account/login");
    }

    [Test]
    public async Task AcceptInvitation_MemberWhoAlreadyHasAnAccount_SignsInAndJoins()
    {
        // Arrange - somebody with an account already, imported into a group they are not in.
        var (group, owner) = await SeedGroup();
        var member = await Provisioning.NewAccount("gs-invited-member");
        await Import(owner, group, member.Email);

        var inviteToken = await MemberChapterInviteDataHelper.GetInviteToken(member.Email, group.ChapterId);

        var acceptPage = new AcceptInvitePage(Page);
        await acceptPage.Open(group.Slug, inviteToken);

        /* Assert - no password form. Offering one would be asking for a second password on an account that
           already has one, so the page asks them to sign in instead. */
        (await acceptPage.HasAcceptForm()).Should().BeFalse();
        (await acceptPage.HasSignInPrompt()).Should().BeTrue();

        // Act - sign in from the prompt, whose return URL brings them back to the invitation.
        await acceptPage.FollowSignInPrompt();
        await new LoginPage(Page).LogInOnCurrentPage(member.Email, member.Password);

        // A signed-in visitor needs no account raising, so the page sends them to the ordinary join page.
        await Page.WaitForURLAsync(url => url.Contains($"/groups/{group.Slug}/join"));

        await new JoinGroupPage(Page).Join(group.Slug);

        // Assert - joined, and the invitation is consumed.
        (await ChapterDataHelper.IsMember(member.Email, group.ChapterId)).Should().BeTrue();
        (await MemberChapterInviteDataHelper.HasInvite(member.Email, group.ChapterId)).Should().BeFalse();
    }

    [Test]
    public async Task AcceptInvitation_TokenThatNamesNoInvitation_OffersNoForm()
    {
        // Arrange - a link already used, or one for another group: from this page they are the same thing.
        var (group, _) = await SeedGroup();

        var acceptPage = new AcceptInvitePage(Page);

        // Act
        await acceptPage.Open(group.Slug, $"not-a-real-token-{Guid.NewGuid():N}");

        // Assert - nothing to fill in, and no sign-in prompt either: the page names nobody to sign in as.
        (await acceptPage.HasAcceptForm()).Should().BeFalse();
        (await acceptPage.HasSignInPrompt()).Should().BeFalse();
    }

    /// <summary>
    /// A group of its own per test: importing writes membership and invitation state to it, which the
    /// isolation rules keep local rather than shared.
    /// </summary>
    private static async Task<(TestGroup Group, TestAccount Owner)> SeedGroup()
    {
        var owner = await Provisioning.NewAccount("gs-invite-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"E2E Invite {Guid.NewGuid():N}");
        return (group, owner);
    }

    private Task Import(TestAccount owner, TestGroup group, string emailAddress) =>
        Provisioning.ImportMembers(
            owner,
            PlatformRoutes.Default(group),
            PlatformBaseUrl,
            [new MemberImportRow
            {
                EmailAddress = emailAddress,
                FirstName = "Imported",
                LastName = "Member"
            }]);
}
