using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Exceptions;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailAdminServiceTests
{
    private const string HtmlFailure = "Malformed HTML at line 1";

    private const EmailType Type = EmailType.NewMember;

    [Test]
    public static async Task DeleteChapterEmail_WithoutTheFeature_StillRestoresTheDefault()
    {
        // Arrange - deliberately not gated: restoring the default puts the group on the standard email
        // it would have without the feature, so blocking it would strand a group with wording it can
        // neither change nor remove.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Custom</p>",
            Id = Guid.NewGuid(),
            Subject = "Custom",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.DeleteChapterEmail(CreateRequest(chapter, currentMember), Type);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<ChapterEmail>().Should().BeEmpty();
    }

    [Test]
    public static async Task GetChapterEmails_TakesTheRecipientTypeFromTheSiteEmail()
    {
        // Arrange - a ChapterEmail carries no recipient type, so the list reads it from the site's row. The
        // group has overridden this template, which is the case where there is something else to read.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context, EmailRecipientType.Admins);
        CreateSiteEmailSettings(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Custom</p>",
            Id = Guid.NewGuid(),
            Subject = "Custom",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        var email = result.Emails.Single();
        email.RecipientType.Should().Be(EmailRecipientType.Admins);
        email.Email.IsDefault().Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterEmails_ReturnsTheGroupsSettings()
    {
        // Arrange - the settings form is rendered from these, so the page needs them alongside the list.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);
        context.Create(new ChapterEmailSettings
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            MemberTitle = "Our own wording"
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert - the site's titles come through too, since the form shows them beside the group's boxes.
        result.Settings!.MemberTitle.Should().Be("Our own wording");
        result.SiteMemberTitle.Should().Be("Site members");
        result.SiteAdminTitle.Should().Be("Site admins");
        result.CanEdit.Should().BeTrue();
    }

    [Test]
    public static async Task GetChapterEmails_WithoutTheFeature_StillListsTheTemplates()
    {
        // Arrange - the list is navigation, not something the feature withholds: an owner sees which
        // templates exist, and which are already customised, before deciding whether to upgrade.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        result.Emails.Should().ContainSingle(x => x.Email.Type == Type);
        result.CanEdit.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithTheFeature_Succeeds()
    {
        // Arrange
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_ReturnsFailure()
    {
        // Arrange - the form renders read-only without the feature, but that is only presentation. This
        // is the guard, and it is what a posted form has to get past.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_LeavesTheExistingOverrideAlone()
    {
        // Arrange - a group that customised an email before its subscription changed keeps sending what
        // it had. Losing the feature withholds editing; it does not quietly revert the email.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        await service.UpdateChapterEmail(CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().Be("Existing");
        stored.HtmlContent.Should().Be("<p>Existing</p>");
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_BlankTitle_StoresItAsUnset()
    {
        // Arrange - blank is how the form posts a box the group cleared. Stored as null so the row says
        // the group has not set a title, which is what makes it inherit the site's again.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        context.Create(new ChapterEmailSettings
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            MemberTitle = "Previous wording"
        });

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = null,
                MemberTitle = "   "
            });

        // Assert
        result.Success.Should().BeTrue();
        var stored = context.Set<ChapterEmailSettings>().Single(x => x.ChapterId == chapter.Id);
        stored.MemberTitle.Should().BeNull();
        stored.AdminTitle.Should().BeNull();
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_WithTheFeature_StoresTheTitles()
    {
        // Arrange - no row exists until the group saves the form for the first time.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = "Admin wording",
                MemberTitle = "Member wording"
            });

        // Assert
        result.Success.Should().BeTrue();
        var stored = context.Set<ChapterEmailSettings>().Single(x => x.ChapterId == chapter.Id);
        stored.AdminTitle.Should().Be("Admin wording");
        stored.MemberTitle.Should().Be("Member wording");
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_WithoutTheFeature_ReturnsFailure()
    {
        // Arrange - the form renders read-only without the feature, but that is only presentation. This is
        // the guard, and it is what a posted form has to get past.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = "Admin wording",
                MemberTitle = "Member wording"
            });

        // Assert
        result.Success.Should().BeFalse();
        context.Set<ChapterEmailSettings>().Should().BeEmpty();
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_LayoutEmail_SkipsTheCheck()
    {
        // Arrange - the layout is the full HTML document the other emails render into, so the allow-list
        // tuned for rich text would reject it outright. The editor must not flag what the save accepts.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), EmailType.Layout, "<html><body></body></html>");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_MalformedHtml_ReturnsFailure()
    {
        // Arrange
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_NotChapterAdmin_Throws()
    {
        // Arrange - the endpoint takes the content the caller is editing, so it has to be as authorised
        // as the save is: without this, any member could use it to probe another group's admin routes.
        using var context = new MockOdkContext();
        var (chapter, _) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        var outsider = context.CreateMember();

        var service = CreateService(context);

        // Act
        var act = async () => await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, outsider), Type, "<p>Valid</p>");

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_WithoutTheFeature_StillValidates()
    {
        // Arrange - deliberately not gated on the feature, unlike UpdateChapterEmail. Those refusals are
        // about whether a template may be saved at all, and answering them here would put "cannot be
        // customised" under a field as if it were a markup error.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_WritesNothing()
    {
        // Arrange - the editor calls this while the admin types, so it has to be free of side effects:
        // content that never gets submitted must not leave a saved override behind.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        await service.ValidateChapterEmailHtml(CreateRequest(chapter, currentMember), Type, "<p>Valid</p>");

        // Assert
        context.Set<ChapterEmail>().Should().BeEmpty();
    }

    [Test]
    public static void ValidateEmailHtml_MalformedHtml_ReturnsFailure()
    {
        // Arrange
        using var context = new MockOdkContext();
        var currentMember = context.CreateMember(siteAdmin: true);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = service.ValidateEmailHtml(
            CreateMemberRequest(currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static void ValidateEmailHtml_NotSiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        var currentMember = context.CreateMember();

        var service = CreateService(context);

        // Act
        var act = () => service.ValidateEmailHtml(CreateMemberRequest(currentMember), Type, "<p>Valid</p>");

        // Assert
        act.Should().Throw<OdkNotAuthorizedException>();
    }

    private static (Chapter Chapter, Member CurrentMember) CreateChapter(
        MockOdkContext context, bool withFeature)
    {
        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(
                features: withFeature ? [SiteFeatureType.CustomEmails] : []));

        return (chapter, currentMember);
    }

    // Set up explicitly rather than left bare: a Mock.Of with no configured return hands back null from
    // Validate, which reads as a pass and turns every failure case into a false pass.
    private static IHtmlValidator CreateHtmlValidator(ServiceResult result) => Mock.Of<IHtmlValidator>(x =>
        x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()) == result);

    private static IHttpRequestContext CreateHttpRequestContext()
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.BaseUrl).Returns("https://test.local");
        return mock.Object;
    }

    private static IMemberServiceRequest CreateMemberRequest(Member currentMember)
    {
        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(currentMember);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        mock.Setup(x => x.HttpRequestContext).Returns(CreateHttpRequestContext());
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        return mock.Object;
    }

    private static IMemberChapterAdminServiceRequest CreateRequest(Chapter chapter, Member currentMember)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();
        mock.Setup(x => x.Chapter).Returns(chapter);
        mock.Setup(x => x.CurrentMember).Returns(currentMember);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        mock.Setup(x => x.HttpRequestContext).Returns(CreateHttpRequestContext());
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        mock.Setup(x => x.Securable).Returns(ChapterAdminSecurable.Emails);
        return mock.Object;
    }

    private static EmailAdminService CreateService(
        MockOdkContext context, IHtmlValidator? htmlValidator = null) => new(
        MockUnitOfWork.Create(context),
        Mock.Of<IMemberEmailService>(),
        // The real one, not a mock: it has no dependencies and is a pure function over the arranged
        // subscription features. A bare mock returns false from every check, which turns the
        // with-the-feature cases into false passes.
        new AuthorizationService(),
        htmlValidator ?? CreateHtmlValidator(ServiceResult.Successful()));

    private static void CreateSiteEmail(
        MockOdkContext context,
        EmailRecipientType recipientType = EmailRecipientType.Members) => context.Create(new Email
    {
        HtmlContent = "<p>Standard</p>",
        Overridable = true,
        RecipientType = recipientType,
        Subject = "Standard",
        Type = Type
    });

    /* The chapter email pages read the site's titles, to show a group what leaving a box empty gives it, so
       the settings row has to exist for them to load at all. */
    private static void CreateSiteEmailSettings(MockOdkContext context) => context.Create(new SiteEmailSettings
    {
        AdminTitle = "Site admins",
        FromEmailAddress = "noreply@example.com",
        FromName = "Site",
        Id = Guid.NewGuid(),
        MemberTitle = "Site members",
        Platform = PlatformType.Default,
        PlatformTitle = "Platform",
        Title = "Site"
    });

    private static EmailUpdateModel CreateUpdateModel() => new()
    {
        HtmlContent = "<p>Updated</p>",
        Overridable = false,
        Subject = "Updated"
    };
}
