using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A group that vets who joins: applying leaves the member waiting, and an admin lets them in. These are the
/// membership machine's PendingApproval edges - joining into it, and Approve out of it.
/// </summary>
/// <remarks>
/// <para>
/// Group Squirrel only. The approvals route is declared <c>PlatformType.Default</c> in the app, so Drunken
/// Knitwits has no page to drive.
/// </para>
/// <para>
/// Every test provisions its own group and its own member. Approval is dynamic, multi-actor state - exactly
/// what the isolation rules say must be local - and the group's membership settings are mutated to arrange
/// it, which rules a shared group out twice over.
/// </para>
/// </remarks>
[TestFixture]
[Category("ChapterMembershipWorkflows")]
public class MemberApprovalTests : DefaultPageTest
{
    /// <summary>
    /// Mirrors the subject the site's member-approved email is seeded with. Held here so a wording change is
    /// a one-line update rather than a hunt through assertions.
    /// </summary>
    private const string ApprovedSubjectFragment = "approved";

    private static ChapterDataHelper Chapters => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static SentEmailDataHelper SentEmails => new(E2ESettings.ConnectionString);

    [Test]
    public async Task JoinGroup_GroupRequiringApproval_LeavesTheMemberAwaitingApproval()
    {
        // Arrange - a group that vets new members.
        var (group, _, member) = await GroupWithAMemberAwaitingApproval();

        // Assert - the membership row exists from the moment they apply, but is not approved.
        (await Chapters.IsMember(member.Email, group.ChapterId))
            .Should().BeTrue("applying writes the membership row");
        (await Chapters.IsApprovedMember(member.Email, group.ChapterId))
            .Should().BeFalse("a group that vets new members does not admit them on joining");
    }

    [Test]
    public async Task ApproveMember_AwaitingApproval_AdmitsThemAndTellsThem()
    {
        // Arrange - a group that vets new members, and somebody waiting.
        var (group, owner, member) = await GroupWithAMemberAwaitingApproval();
        var memberId = await Members.GetMemberId(member.Email);

        // The member's mailbox before approval - activation and welcome, from creating the account.
        var before = await SentEmails.GetSubjects(member.Email, expectedCount: 2);

        // Act - the owner approves them from the approvals page.
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        var approvals = new MemberApprovalsAdminPage(Page);
        (await approvals.IsAwaitingApproval(group.ChapterId, memberId))
            .Should().BeTrue("the member should be listed before being approved");

        await approvals.Approve(group.ChapterId, memberId);

        // Assert - they are in, off the list, and have been told.
        (await Chapters.IsApprovedMember(member.Email, group.ChapterId)).Should().BeTrue();
        (await approvals.IsAwaitingApproval(group.ChapterId, memberId))
            .Should().BeFalse("an approved member is no longer waiting");

        var after = await SentEmails.GetSubjects(member.Email, expectedCount: before.Count + 1);
        after.Except(before).Should().Contain(
            x => x.Contains(ApprovedSubjectFragment, StringComparison.OrdinalIgnoreCase),
            $"Expected an approval email. Before: [{string.Join(", ", before)}]; after: [{string.Join(", ", after)}]");
    }

    [Test]
    public async Task ApproveMember_AlreadyApproved_SucceedsWithoutTellingThemAgain()
    {
        /* Approving somebody already in is a no-op that still reports success - the machine has an Approve
           edge out of Joined carrying no steps. The UI drops them off the approvals list once approved, so
           the second approval has to go at the endpoint directly. */
        // Arrange - a group that vets new members, and a member already approved once.
        var (group, owner, member) = await GroupWithAMemberAwaitingApproval();
        var memberId = await Members.GetMemberId(member.Email);

        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        await new MemberApprovalsAdminPage(Page).Approve(group.ChapterId, memberId);

        var afterFirst = await SentEmails.GetSubjects(member.Email, expectedCount: 3);
        afterFirst.Should().Contain(
            x => x.Contains(ApprovedSubjectFragment, StringComparison.OrdinalIgnoreCase),
            "the first approval should have emailed the member");

        // Act - the owner approves them a second time.
        var status = await ApiRequests.Post(
            Page,
            $"/groups/{group.ChapterId}/members/{memberId}/approve",
            "/account");

        // Assert - the endpoint ran and reported no failure, and the member stays in.
        status.Should().NotBe(400, "antiforgery should have passed so the endpoint actually ran");
        (await Chapters.IsApprovedMember(member.Email, group.ChapterId)).Should().BeTrue();

        /* And nothing was sent a second time. This asks for one more email than exists, so it polls out
           before returning what did arrive - the cost of proving an absence against a background job. */
        var afterSecond = await SentEmails.GetSubjects(member.Email, expectedCount: afterFirst.Count + 1);
        afterSecond.Should().HaveCount(
            afterFirst.Count,
            $"approving twice should send one email, not two. Subjects: [{string.Join(", ", afterSecond)}]");
    }

    /// <summary>
    /// A published group whose owner can vet new members and has turned that on, plus a fresh member who has
    /// applied and is waiting. The member's browser is a throwaway one, so the test's own browser is left
    /// anonymous for the admin to sign in on.
    /// </summary>
    private static async Task<(TestGroup Group, TestAccount Owner, TestAccount Member)>
        GroupWithAMemberAwaitingApproval()
    {
        var subscription = await Provisioning.EnsureMemberApprovalSiteSubscription();

        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroup(owner, $"E2E {Guid.NewGuid():N}");

        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, subscription.Id, subscription.PriceId);

        // The feature only makes the setting reachable; the group still has to turn it on.
        await Provisioning.RequireMemberApproval(owner, group.ChapterId);

        var member = await Provisioning.JoinGroupAsMember(group);

        return (group, owner, member);
    }
}
