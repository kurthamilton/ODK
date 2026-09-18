using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

[TestFixture]
public class GroupOwnerTests : DefaultPageTest
{
    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task PublishGroup_Approved_SetsPublishedTimestamp()
    {
        // Arrange - an approved (but not yet published) group, with the picture publishing requires.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateSubmittedGroup(owner, $"E2E {Guid.NewGuid():N}");
        await Provisioning.ApproveGroup(group.ChapterId);

        var publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().BeNull("group should start unpublished");

        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        await new GroupImageAdminPage(Page).SetPicture(group);

        // Act - the owner publishes it through the UI.
        await new GroupAdminPage(Page).Publish(group);

        // Assert - publishing stamps Chapters.PublishedUtc.
        publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().NotBeNull();
    }

    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task PublishGroup_NoPicture_IsRejected()
    {
        // NB the dashboard offers a link to the picture page in place of the Publish button, so this
        // drives the endpoint directly - the same reason PublishGroup_NotApproved_IsRejected does.
        // Arrange - an approved group that has no picture, creating one no longer asking for it.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateSubmittedGroup(owner, $"E2E {Guid.NewGuid():N}");
        await Provisioning.ApproveGroup(group.ChapterId);

        // Act - the owner POSTs to the publish endpoint directly.
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        var status = await ApiRequests.Post(
            Page,
            $"/groups/{group.ChapterId}/publish",
            "/account");

        // Assert - the endpoint ran (antiforgery passed, so not 400) but the group stays unpublished.
        status.Should().NotBe(400, "antiforgery should have passed so the endpoint actually ran");
        var publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().BeNull();
    }

    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task PublishGroup_NotApproved_IsRejected()
    {
        // NB the service returns a failure (ServiceResult.Failure) rather than throwing, and the UI
        // hides the Publish button while unapproved - so this drives the endpoint directly.
        // Arrange - a freshly created, unapproved group.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroup(owner, $"E2E {Guid.NewGuid():N}");

        // Act - the owner POSTs to the publish endpoint directly.
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);
        var status = await ApiRequests.Post(
            Page,
            $"/groups/{group.ChapterId}/publish",
            "/account");

        // Assert - the endpoint ran (antiforgery passed, so not 400) but the group stays unpublished.
        status.Should().NotBe(400, "antiforgery should have passed so the endpoint actually ran");
        var publishedUtc = await ChapterDataHelper.GetPublishedUtc(group.ChapterId);
        publishedUtc.Should().BeNull();
    }

    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task SubmitGroup_ChecklistComplete_SubmitsForApproval()
    {
        // Arrange - a new group, whose owner works through the checklist the way the UI asks them to.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroup(owner, $"E2E {Guid.NewGuid():N}");
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        /* The picture, and with it the privacy settings: the picture is a panel of the settings page, and
           opening that page is the only evidence there is that an organiser considered privacy - its
           settings have defaults, so there is nothing for the checklist to detect. */
        await new GroupImageAdminPage(Page).SetPicture(group);

        /* Likewise the membership settings, which the group only has on its checklist at all when its
           owner's subscription carries MemberSubscriptions. Opened either way, so the step being on the
           checklist or not is the app's business rather than this test's. */
        await new MembershipSettingsAdminPage(Page).Open(group);

        await new GroupTextsAdminPage(Page).Describe(
            group, shortDescription: "An E2E group", description: "Somewhere to test things.");

        // The three optional steps, skipped - which is what the checklist means by dealing with them.
        var groupAdminPage = new GroupAdminPage(Page);
        await groupAdminPage.DismissChecklistItem(group, ChecklistItemTypeIds.Questions);
        await groupAdminPage.DismissChecklistItem(group, ChecklistItemTypeIds.MemberProperties);
        await groupAdminPage.DismissChecklistItem(group, ChecklistItemTypeIds.Topics);

        var canSubmit = await groupAdminPage.CanSubmitForApproval(group);
        canSubmit.Should().BeTrue("every step above submission has been finished or skipped");

        // Act - the owner submits it.
        await groupAdminPage.SubmitForApproval(group);

        // Assert - submitting stamps Chapters.SubmittedForApprovalUtc, which is what a site admin acts on.
        var submittedUtc = await ChapterDataHelper.GetSubmittedForApprovalUtc(group.ChapterId);
        submittedUtc.Should().NotBeNull();
    }

    [Test]
    [Category("ChapterPublicationWorkflows")]
    public async Task SubmitGroup_ChecklistIncomplete_IsNotOffered()
    {
        // Arrange - a group whose owner has done nothing since creating it.
        var owner = await Provisioning.NewAccount(SharedAccounts.GroupOwner);
        var group = await Provisioning.CreateGroup(owner, $"E2E {Guid.NewGuid():N}");
        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act
        var canSubmit = await new GroupAdminPage(Page).CanSubmitForApproval(group);

        // Assert - the checklist says what is holding it up rather than offering the button, and nothing
        // was submitted.
        canSubmit.Should().BeFalse();
        var submittedUtc = await ChapterDataHelper.GetSubmittedForApprovalUtc(group.ChapterId);
        submittedUtc.Should().BeNull();
    }
}