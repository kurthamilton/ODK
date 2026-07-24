using FluentAssertions;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using ODK.E2E.Core;
using ODK.E2E.Data;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

[TestFixture]
[Category("E2E")]
[Explicit("Requires a running Group Squirrel instance, its database, and installed Playwright browsers.")]
public class GroupOwnerTests : PageTest
{
    [Test]
    public async Task PublishGroup_Approved_SetsPublishedTimestamp()
    {
        // Arrange - an approved (but not yet published) group.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroupAsync(owner, $"E2E {Guid.NewGuid():N}");
        await Provisioning.ApproveGroupAsync(group.ChapterId);

        var publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().BeNull("group should start unpublished");

        // Act - the owner publishes it through the UI.
        await new LoginPage(Page).LogInAsync(owner.Email, owner.Password);
        await new GroupAdminPage(Page).PublishAsync(group.ChapterId);

        // Assert - publishing stamps Chapters.PublishedUtc.
        publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().NotBeNull();
    }

    [Test]
    public async Task PublishGroup_NotApproved_IsRejected()
    {
        // NB the service returns a failure (ServiceResult.Failure) rather than throwing, and the UI
        // hides the Publish button while unapproved - so this drives the endpoint directly.
        // Arrange - a freshly created, unapproved group.
        var owner = await Provisioning.NewAccountAsync(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroupAsync(owner, $"E2E {Guid.NewGuid():N}");

        // Act - the owner POSTs to the publish endpoint directly.
        await new LoginPage(Page).LogInAsync(owner.Email, owner.Password);
        var status = await ApiRequests.PostAsync(
            Page,
            $"/groups/{group.ChapterId}/publish",
            "/account");

        // Assert - the endpoint ran (antiforgery passed, so not 400) but the group stays unpublished.
        status.Should().NotBe(400, "antiforgery should have passed so the endpoint actually ran");
        var publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().BeNull();
    }
}