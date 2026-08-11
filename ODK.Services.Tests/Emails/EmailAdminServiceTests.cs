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
using ODK.Services.Members;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailAdminServiceTests
{
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
    public static async Task GetChapterEmails_WithoutTheFeature_StillListsTheTemplates()
    {
        // Arrange - the list is navigation, not something the feature withholds: an owner sees which
        // templates exist, and which are already customised, before deciding whether to upgrade.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        result.Should().ContainSingle(x => x.Type == Type);
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

    private static IHttpRequestContext CreateHttpRequestContext()
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.BaseUrl).Returns("https://test.local");
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

    private static EmailAdminService CreateService(MockOdkContext context) => new(
        MockUnitOfWork.Create(context),
        Mock.Of<IMemberEmailService>(),
        // The real one, not a mock: it has no dependencies and is a pure function over the arranged
        // subscription features. A bare mock returns false from every check, which turns the
        // with-the-feature cases into false passes.
        new AuthorizationService());

    private static void CreateSiteEmail(MockOdkContext context) => context.Create(new Email
    {
        HtmlContent = "<p>Standard</p>",
        Overridable = true,
        Subject = "Standard",
        Type = Type
    });

    private static EmailUpdateModel CreateUpdateModel() => new()
    {
        HtmlContent = "<p>Updated</p>",
        Overridable = false,
        Subject = "Updated"
    };
}
