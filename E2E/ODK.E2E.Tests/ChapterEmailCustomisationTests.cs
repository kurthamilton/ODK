using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A group customising one of its email templates. Subject and body are independent: customising one leaves
/// the other on the site's default and following any later change to it, which is the behaviour these cover -
/// through the form, through what is stored, and through what an actual send puts on the wire.
/// <para>
/// Which template gets customised comes from the app rather than from these tests: the group's list carries
/// the group emails - the ones a group may override - so its first row is always a valid pick. Nothing here
/// writes to the site's own <c>Emails</c>, which is configuration these tests read through the UI rather
/// than arrange.
/// </para>
/// <para>
/// Every test provisions its own group. Customising writes the chapter's own state, so a shared chapter is
/// not an option (see the isolation rules in CLAUDE.md).
/// </para>
/// </summary>
[TestFixture]
[Category("EmailAdmin")]
public class ChapterEmailCustomisationTests : DefaultPageTest
{
    private static ChapterEmailDataHelper ChapterEmails => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static SentEmailDataHelper SentEmails => new(E2ESettings.ConnectionString);

    [Test]
    public async Task ChapterEmail_BeforeCustomising_ShowsTheDefaultWordingLocked()
    {
        // Arrange - the form opens on the email the group actually sends, so an owner sees the default rather
        // than an empty box, and cannot type over it until they ask to.
        var (group, owner, page, emailUrl) = await OpenAnEmail();

        // Assert - wording is shown while the group has stored none of its own, which is only possible by
        // falling back to the site's.
        (await ChapterEmails.GetRowCount(group.ChapterId)).Should().Be(0);
        (await page.GetSubject()).Should().NotBeEmpty();
        (await page.GetContent()).Should().NotBeEmpty();

        (await page.IsSubjectEditable()).Should().BeFalse("a field on the default is not typed into");
        (await page.IsContentEditable()).Should().BeFalse();
        (await page.IsCustomised()).Should().BeFalse();
    }

    [Test]
    public async Task ChapterEmail_CustomisingTheSubjectOnly_SendsItWithTheDefaultBody()
    {
        // Arrange - the case the per-field fallback exists for. Only the subject is customised, so the body
        // has to arrive as the site's rather than as blank.
        var (group, owner, page, emailUrl) = await OpenAnEmail();
        var defaultContent = await page.GetContent();

        var customSubject = $"E2E custom subject {Guid.NewGuid():N}";

        // Act
        await page.SetCustomWording(emailUrl, subject: customSubject);

        // Assert - stored as an override of the subject alone; the body is left unset, which is what makes
        // the send fall back rather than sending an empty body.
        (await ChapterEmails.GetSubject(group.ChapterId)).Should().Be(customSubject);
        (await ChapterEmails.GetHtmlContent(group.ChapterId)).Should().BeNull();
        (await page.IsCustomised()).Should().BeTrue();

        // The body field still shows the default, unchanged and still locked.
        await page.Open(emailUrl);
        (await page.GetContent()).Should().Be(defaultContent);
        (await page.IsContentEditable()).Should().BeFalse();

        // And it reaches the wire: a test send uses the group's subject, having taken the body from the site.
        // Counted from what the owner has already been sent - activating the account sent them mail - so the
        // poll waits for this send rather than being satisfied by those.
        var before = await SentEmails.GetSubjects(owner.Email, expectedCount: 0);

        await page.SendTest(emailUrl);

        var subjects = await SentEmails.GetSubjects(owner.Email, expectedCount: before.Count + 1);
        subjects.Should().Contain(customSubject);
    }

    [Test]
    public async Task ChapterEmail_CustomisingTheSubjectOnly_IsListedAsSubjectOnly()
    {
        // Arrange - the list names which fields a group has customised rather than ticking a box, so it has
        // to distinguish one field from both.
        var (group, owner, page, emailUrl) = await OpenAnEmail();
        var listUrl = PlatformRoutes.Default(group).EmailsAdmin;

        (await page.GetListedCustomFields(listUrl)).Should().Be("Default");

        // Act
        await page.SetCustomWording(emailUrl, subject: $"E2E subject {Guid.NewGuid():N}");

        // Assert
        (await page.GetListedCustomFields(listUrl)).Should().Be("Subject");
    }

    [Test]
    public async Task ChapterEmail_TurningCustomisationOff_RestoresTheDefault()
    {
        // Arrange - a group that has customised both fields turns both off again. The row goes rather than
        // being kept as an override of nothing, which would still badge the email as customised.
        var (group, owner, page, emailUrl) = await OpenAnEmail();
        var defaultSubject = await page.GetSubject();

        await page.SetCustomWording(
            emailUrl, subject: "E2E to be removed", htmlContent: "<p>E2E to be removed</p>");

        (await ChapterEmails.GetRowCount(group.ChapterId))
            .Should().Be(1, "the group should start this test customised");

        // Act
        await page.RestoreDefaults(emailUrl);

        // Assert
        (await ChapterEmails.GetRowCount(group.ChapterId)).Should().Be(0);
        (await page.IsCustomised()).Should().BeFalse();

        // The form is back to showing the default, locked.
        await page.Open(emailUrl);
        (await page.GetSubject()).Should().Be(defaultSubject);
        (await page.IsSubjectEditable()).Should().BeFalse();
    }

    [Test]
    public async Task ChapterEmail_CustomisingOneFieldThenTheOther_KeepsBoth()
    {
        // Arrange - the two fields are stored independently, so customising the body later must not disturb
        // the subject that is already overridden.
        var (group, owner, page, emailUrl) = await OpenAnEmail();

        var customSubject = $"E2E subject {Guid.NewGuid():N}";
        var customContent = $"<p>E2E body {Guid.NewGuid():N}</p>";

        await page.SetCustomWording(emailUrl, subject: customSubject);

        // Act - a second visit customises the body, leaving the subject switch as it was found.
        await page.SetCustomWording(emailUrl, htmlContent: customContent);

        // Assert
        (await ChapterEmails.GetSubject(group.ChapterId)).Should().Be(customSubject);
        (await ChapterEmails.GetHtmlContent(group.ChapterId)).Should().Be(customContent);

        // Both fields now hold the group's own wording, and both may be typed into.
        await page.Open(emailUrl);
        (await page.GetSubject()).Should().Be(customSubject);
        (await page.GetContent()).Should().Be(customContent);
        (await page.IsSubjectEditable()).Should().BeTrue();
        (await page.IsContentEditable()).Should().BeTrue();
    }

    [Test]
    public async Task ChapterEmail_WithoutTheFeature_CanTurnCustomisationOffButNotWriteIt()
    {
        // Arrange - a group that customised an email and then lost the feature. It keeps sending what it has
        // and can still go back to the default; what it cannot do is write new wording.
        var (group, owner, page, emailUrl) = await OpenAnEmail();

        await page.SetCustomWording(emailUrl, subject: $"E2E stranded {Guid.NewGuid():N}");

        // The subscription lapses, taking the feature with it.
        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.Expire(ownerId);

        // Act
        await page.Open(emailUrl);

        // Assert - the wording is shown but locked, while the switch that releases the group from it works.
        (await page.IsSubjectEditable()).Should().BeFalse("wording cannot be written without the feature");
        (await page.IsContentToggleEnabled())
            .Should().BeFalse("a field on the default has nothing to turn off");

        await page.RestoreDefaults(emailUrl);

        (await ChapterEmails.GetRowCount(group.ChapterId))
            .Should().Be(0, "turning customisation off stays available without the feature");
    }

    /// <summary>
    /// Provisions a group whose subscription covers custom emails, signs its owner in, and opens whichever
    /// template the app lists first. Returns the page object and that template's path.
    /// </summary>
    private async Task<(TestGroup Group, TestAccount Owner, ChapterEmailAdminPage Page, string EmailUrl)> OpenAnEmail()
    {
        var subscription = await Provisioning.EnsureCustomEmailsSiteSubscription();

        var owner = await Provisioning.NewAccount("email-custom-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2eemail{Guid.NewGuid():N}");

        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, subscription.Id, subscription.PriceId);

        await new LoginPage(Page).LogIn(owner.Email, TestAccounts.Password);

        var page = new ChapterEmailAdminPage(Page);
        var emailUrl = await page.OpenFirstEmail(PlatformRoutes.Default(group).EmailsAdmin);

        return (group, owner, page, emailUrl);
    }
}
