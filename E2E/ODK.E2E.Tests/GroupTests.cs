using FluentAssertions;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

[TestFixture]
[Category("E2E")]
[Explicit("Requires a running Group Squirrel instance, its database, and installed Playwright browsers.")]
public class GroupTests : PageTest
{
    [Test]
    public async Task JoinGroup_GroupNotApproved_IsBlocked()
    {
        // An unapproved group is also unpublished, so the join page is blocked by the "Group not
        // published" guard - there's no distinct "not approved" rejection on the join path. The guard
        // throws OdkNotFoundException server-side, which the error middleware re-executes into a
        // "Not found" page returned as HTTP 200 (not a 404 status), so assert on the blocked content.
        // Arrange - a freshly created, unapproved group and a logged-in member.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        var member = await SharedAccounts.GetAsync(SharedAccounts.GroupMember);
        await new LoginPage(Page).LogInAsync(member.Email, member.Password);

        // Act.
        await Page.Navigate($"/groups/{group.Slug}/join");
        var body = await Page.InnerTextAsync("body");

        // Assert - the join page is blocked (no join form) and no membership was created.
        body.ToLowerInvariant().Should().Contain("not found", $"expected the join page to be blocked; body={body[..Math.Min(400, body.Length)]}");
        body.Should().NotContain("Join group");

        var isMember = await ChapterDataHelper.IsMember(member.Email, group.ChapterId);
        isMember.Should().BeFalse();
    }

    [Test]
    public async Task JoinGroup_GroupNotPublished_IsBlocked()
    {
        // Arrange - an approved but not-yet-published group and a logged-in member.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        await Provisioning.ApproveGroupAsync(group.ChapterId);
        var member = await SharedAccounts.GetAsync(SharedAccounts.GroupMember);
        await new LoginPage(Page).LogInAsync(member.Email, member.Password);

        // Act.
        await Page.Navigate($"/groups/{group.Slug}/join");
        var body = await Page.InnerTextAsync("body");

        // Assert - the join page is blocked (no join form) and no membership was created.
        body.ToLowerInvariant().Should().NotContain("Not found", $"expected the join page to be blocked; body={body[..Math.Min(400, body.Length)]}");
        body.Should().NotContain("Join group");

        var isMember = await ChapterDataHelper.IsMember(member.Email, group.ChapterId);
        isMember.Should().BeFalse();
    }

    [Test]
    public async Task JoinGroup_GroupPublished_AddsMemberToGroup()
    {
        // Arrange - a published group and a fresh member (each member can only join once).
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        var member = await Provisioning.NewAccountAsync(SharedAccounts.GroupMember);

        // Act - the member joins through the UI.
        await new LoginPage(Page).LogInAsync(member.Email, member.Password);
        await new JoinGroupPage(Page).JoinAsync(group.Slug);

        // Assert - a MemberChapters row now links the member to the group.
        var isMember = await ChapterDataHelper.IsMember(member.Email, group.ChapterId);
        isMember.Should().BeTrue();
    }

    [Test]
    public async Task JoinGroup_GroupPublished_SendsNewMemberEmailToOwner()
    {
        // A fresh owner isolates the owner's mailbox. Joining sends the owner a NewMemberAdmin email;
        // the subject is DB-seeded, so assert a *new* subject appears for the owner rather than
        // hard-coding it. Before joining, the owner already has activation + welcome + group-approved.
        // Arrange.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreatePublishedGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        var member = await Provisioning.NewAccountAsync(SharedAccounts.GroupMember);

        var before = await SentEmailDataHelper.GetSubjects(owner.Email, expectedCount: 3);

        // Act - the member joins.
        await new LoginPage(Page).LogInAsync(member.Email, member.Password);
        await new JoinGroupPage(Page).JoinAsync(group.Slug);

        // Assert - the owner received an additional (new-member) email.
        var after = await SentEmailDataHelper.GetSubjects(owner.Email, expectedCount: before.Count + 1);
        var newSubjects = after.Except(before).ToArray();
        newSubjects.Should().NotBeEmpty(
            $"Expected a new-member email to the owner. Before: [{string.Join(", ", before)}]; after: [{string.Join(", ", after)}]");
    }
}