using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

[TestFixture]
public class SiteAdminTests : DefaultPageTest
{
    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task ApproveGroup_SetsApprovedUtcTimestamp()
    {
        // Arrange - a group owner creates a group, which starts unapproved. A fresh owner each time,
        // because the default (Free) subscription's group limit is 1.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroup(owner, $"E2E {Guid.NewGuid():N}");
        var approvedUtc = await ChapterDataHelper.GetApprovedUtc(group.ChapterId);
        approvedUtc.Should().BeNull("group should start unapproved");

        // Act - a site admin approves it through the UI.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);
        await new SiteAdminGroupsPage(Page).Approve(group.ChapterId);

        // Assert - approval stamps Chapters.ApprovedUtc.
        approvedUtc = await ChapterDataHelper.GetApprovedUtc(group.ChapterId);
        approvedUtc.Should().NotBeNull();
    }
}