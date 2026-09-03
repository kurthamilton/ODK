using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Core.Platforms;
using ODK.Data.Core.Events;
using ODK.Services.Sitemap.ViewModels;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Tests.Routes;

/// <summary>
/// The registry resolved against what a group has configured. A page a visitor cannot open is a 404 or an
/// empty page offered to a crawler, so each condition the registry declares is checked in both directions.
/// </summary>
[Parallelizable]
public static class PublicRoutesSitemapTests
{
    private static readonly DateTime PublishedUtc = new(2026, 3, 1, 9, 30, 0, DateTimeKind.Utc);

    [Test]
    public static void Sitemap_DefaultPlatform_OmitsTheDrunkenKnitwitsAboutPage()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;
        var chapter = CreateChapter(PlatformType.Default);

        // Act - Group Squirrel's About is the group home page, so there is no /about page to offer.
        var result = Paths(routes, chapter);

        // Assert
        result.Should().Contain("/groups/{slug}");
        result.Should().NotContain("/groups/{slug}/about");
    }

    [Test]
    public static void Sitemap_DefaultPlatform_OmitsTheFaqPageWhenTheGroupHasNoQuestions()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;
        var chapter = CreateChapter(PlatformType.Default);

        // Act
        var withQuestions = Paths(routes, chapter, hasQuestions: true);
        var withoutQuestions = Paths(routes, chapter, hasQuestions: false);

        // Assert
        withQuestions.Should().Contain("/groups/{slug}/faq");
        withoutQuestions.Should().NotContain("/groups/{slug}/faq");
    }

    [Test]
    public static void Sitemap_DrunkenKnitwits_IncludesTheSiteContactPage()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.DrunkenKnitwits).Public;
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits);

        // Act - /contact is a Drunken Knitwits page: its footer links it.
        var result = Paths(routes, chapter);

        // Assert
        result.Should().Contain("/contact");
        result.Should().Contain("/{chaptername}");
        result.Should().Contain("/{chaptername}/about");
    }

    [Test]
    public static void Sitemap_DrunkenKnitwits_OmitsTheGroupSquirrelPages()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.DrunkenKnitwits).Public;
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits);

        // Act
        var result = Paths(routes, chapter, hasQuestions: true, eventsArePublic: true);

        // Assert - the site About page, the group directory, the pricing page, the FAQ page and the past
        // events page all belong to Group Squirrel. Drunken Knitwits reaches About through its own
        // per-chapter page instead.
        result.Should().NotContain("/about");
        result.Should().NotContain("/groups");
        result.Should().NotContain("/pricing");
        result.Should().NotContain("/{chaptername}/faq");
        result.Should().NotContain("/{chaptername}/events/past");
    }

    [Test]
    public static void Sitemap_EventsArePublic_IncludesEachEventWithItsPublicationDate()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;
        var chapter = CreateChapter(PlatformType.Default);

        // Act
        var result = routes.Sitemap(CreateViewModel(chapter, eventsArePublic: true, shortcodes: ["abc123"]));

        // Assert
        var node = result.Single(x => x.Path == "/groups/{slug}/events/abc123");
        node.LastModifiedUtc.Should().Be(PublishedUtc);
    }

    [Test]
    public static void Sitemap_EventsAreNotPublic_OmitsTheEventPages()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;
        var chapter = CreateChapter(PlatformType.Default);

        // Act - an anonymous visitor resolves to Public visibility, so a group showing its events to
        // members only shows a crawler an empty events page and a not-found for each event.
        var result = Paths(routes, chapter, eventsArePublic: false, shortcodes: ["abc123"]);

        // Assert
        result.Should().NotContain("/groups/{slug}/events");
        result.Should().NotContain("/groups/{slug}/events/past");
        result.Should().NotContain("/groups/{slug}/events/abc123");
    }

    [Test]
    public static void Sitemap_HiddenAboutPage_OmitsThePageButKeepsTheGroupHomePage()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.DrunkenKnitwits).Public;
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits);

        // Act
        var result = Paths(routes, chapter, hiddenPages: [PageType.About]);

        // Assert
        result.Should().NotContain("/{chaptername}/about");
        result.Should().Contain("/{chaptername}");
    }

    [Test]
    public static void Sitemap_HiddenContactPage_OmitsThePage()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;
        var chapter = CreateChapter(PlatformType.Default);

        // Act
        var result = Paths(routes, chapter, hiddenPages: [PageType.Contact]);

        // Assert
        result.Should().NotContain("/groups/{slug}/contact");
    }

    [Test]
    public static void Sitemap_NoChapters_StillListsTheSitePages()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.Default).Public;

        // Act
        var result = routes
            .Sitemap(new SitemapViewModel { Chapters = [] })
            .Select(x => x.Path)
            .ToArray();

        // Assert
        result.Should().BeEquivalentTo(
            ["/", "/groups", "/about", "/contact", "/pricing", "/privacy"]);
    }

    /// <summary>
    /// A chapter whose route values are the names its pages declare, so the paths the registry builds read
    /// as the route templates they are.
    /// </summary>
    private static Chapter CreateChapter(PlatformType platform) => new()
    {
        Id = Guid.NewGuid(),
        Name = "{chapterName}",
        Platform = platform,
        Slug = "{slug}"
    };

    private static SitemapViewModel CreateViewModel(
        Chapter chapter,
        bool hasQuestions = false,
        bool eventsArePublic = false,
        IReadOnlyCollection<PageType>? hiddenPages = null,
        IReadOnlyCollection<string>? shortcodes = null) => new()
        {
            Chapters =
            [
                new SitemapChapterViewModel
                {
                    Chapter = chapter,
                    ChapterPages = (hiddenPages ?? [])
                        .Select(x => new ChapterPage
                        {
                            ChapterId = chapter.Id,
                            Hidden = true,
                            PageType = x
                        })
                        .ToArray(),
                    Events = eventsArePublic
                        ? (shortcodes ?? [])
                            .Select(x => new EventPublicationDto
                            {
                                ChapterId = chapter.Id,
                                PublishedUtc = PublishedUtc,
                                Shortcode = x
                            })
                            .ToArray()
                        : [],
                    HasQuestions = hasQuestions,
                    PrivacySettings = new ChapterPrivacySettings
                    {
                        ChapterId = chapter.Id,
                        EventVisibility = eventsArePublic
                            ? ChapterFeatureVisibilityType.Public
                            : ChapterFeatureVisibilityType.ActiveMembers
                    }
                }
            ]
        };

    private static IReadOnlyCollection<string> Paths(
        PublicRoutes routes,
        Chapter chapter,
        bool hasQuestions = false,
        bool eventsArePublic = false,
        IReadOnlyCollection<PageType>? hiddenPages = null,
        IReadOnlyCollection<string>? shortcodes = null)
        => routes
            .Sitemap(CreateViewModel(chapter, hasQuestions, eventsArePublic, hiddenPages, shortcodes))
            .Select(x => x.Path)
            .ToArray();
}
