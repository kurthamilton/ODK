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
public class SiteAdminTests : PageTest
{
    [Test]
    public async Task ApproveGroup_SetsApprovedUtcTimestamp()
    {
        // Arrange - a group owner creates a group, which starts unapproved. A fresh owner each time,
        // because the default (Free) subscription's group limit is 1.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        var approvedUtc = await ChapterDataHelper.GetApprovedUtc(group.ChapterId);
        approvedUtc.Should().BeNull("group should start unapproved");

        // Act - a site admin approves it through the UI.
        var admin = await SharedAccounts.GetAsync(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogInAsync(admin.Email, admin.Password);
        await new SiteAdminGroupsPage(Page).ApproveAsync(group.ChapterId);

        // Assert - approval stamps Chapters.ApprovedUtc.
        approvedUtc = await ChapterDataHelper.GetApprovedUtc(group.ChapterId);
        approvedUtc.Should().NotBeNull();
    }
}