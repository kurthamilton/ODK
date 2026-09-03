using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Pages;
using ODK.Core.Platforms;
using ODK.Services.Sitemap;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Sitemap;

[Parallelizable]
public static class SitemapServiceTests
{
    [Test]
    public static async Task GetSitemapViewModel_ChapterShowsEventsPublicly_ReturnsThePublishedEvents()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = CreateChapter(context);
        SetEventVisibility(context, chapter, ChapterFeatureVisibilityType.Public);

        var published = context.CreateEvent(chapter);
        published.Shortcode = "abc123";

        var draft = context.CreateEvent(chapter);
        draft.Shortcode = "def456";
        draft.PublishedUtc = null;

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.Default));

        // Assert
        result.Chapters.Single().Events
            .Select(x => x.Shortcode)
            .Should().BeEquivalentTo(["abc123"]);
    }

    [Test]
    public static async Task GetSitemapViewModel_ChapterShowsEventsToMembersOnly_ReturnsNoEvents()
    {
        // Arrange - the default visibility is ActiveMembers, so a group that has said nothing shows its
        // events to no anonymous visitor and there is nothing to offer a crawler.
        using var context = new MockOdkContext();

        var chapter = CreateChapter(context);
        var @event = context.CreateEvent(chapter);
        @event.Shortcode = "abc123";

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.Default));

        // Assert
        result.Chapters.Single().Events.Should().BeEmpty();
    }

    [Test]
    public static async Task GetSitemapViewModel_ReturnsChaptersInDisplayNameOrder()
    {
        // Arrange
        using var context = new MockOdkContext();

        CreateChapter(context, name: "Zebras");
        CreateChapter(context, name: "Aardvarks");

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.Default));

        // Assert
        result.Chapters
            .Select(x => x.Chapter.Name)
            .Should().ContainInOrder("Aardvarks", "Zebras");
    }

    [Test]
    public static async Task GetSitemapViewModel_ReturnsEachChaptersOwnPagesAndQuestions()
    {
        // Arrange
        using var context = new MockOdkContext();

        var withQuestions = CreateChapter(context, name: "Aardvarks");
        var withHiddenPage = CreateChapter(context, name: "Zebras");

        context.Create(new ChapterQuestion
        {
            AnswerHtml = "Yes",
            ChapterId = withQuestions.Id,
            Id = Guid.NewGuid(),
            Name = "Is it?"
        });

        context.Create(new ChapterPage
        {
            ChapterId = withHiddenPage.Id,
            Hidden = true,
            Id = Guid.NewGuid(),
            PageType = PageType.Contact
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.Default));

        // Assert
        var first = result.Chapters.First();
        first.HasQuestions.Should().BeTrue();
        first.ChapterPages.Should().BeEmpty();

        var second = result.Chapters.Last();
        second.HasQuestions.Should().BeFalse();
        second.ChapterPages.Single().PageType.Should().Be(PageType.Contact);
    }

    [Test]
    public static async Task GetSitemapViewModel_UnapprovedChapter_IsExcluded()
    {
        // Arrange
        using var context = new MockOdkContext();

        CreateChapter(context, approved: false);

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.Default));

        // Assert
        result.Chapters.Should().BeEmpty();
    }

    [Test]
    public static async Task GetSitemapViewModel_UnpublishedDrunkenKnitwitsChapter_IsExcluded()
    {
        // Arrange - Drunken Knitwits chapter queries do not filter on publication, so an unpublished
        // chapter reaches the service. Its pages render a registration-closed stub.
        using var context = new MockOdkContext();

        CreateChapter(context, platform: PlatformType.DrunkenKnitwits, published: false);

        var service = CreateService(context);

        // Act
        var result = await service.GetSitemapViewModel(CreateServiceRequest(PlatformType.DrunkenKnitwits));

        // Assert
        result.Chapters.Should().BeEmpty();
    }

    private static Chapter CreateChapter(
        MockOdkContext context,
        string name = "Test group",
        PlatformType platform = PlatformType.Default,
        bool approved = true,
        bool published = true)
        => context.CreateChapter(
            approvedUtc: approved ? DateTime.UtcNow : null,
            name: name,
            platform: platform,
            afterCreate: x => x.PublishedUtc = published ? DateTime.UtcNow : null);

    private static IServiceRequest CreateServiceRequest(PlatformType platform)
    {
        var mock = new Mock<IServiceRequest>();

        mock.Setup(x => x.Platform)
            .Returns(platform);

        return mock.Object;
    }

    private static SitemapService CreateService(MockOdkContext context)
        => new(MockUnitOfWorkFactory.Create(context));

    private static void SetEventVisibility(
        MockOdkContext context,
        Chapter chapter,
        ChapterFeatureVisibilityType visibility)
        => context.Create(new ChapterPrivacySettings
        {
            ChapterId = chapter.Id,
            EventVisibility = visibility
        });
}
