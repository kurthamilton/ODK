using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Tests.Routes;

[Parallelizable]
public static class AccountRoutesTests
{
    [Test]
    public static void Pending_DrunkenKnitwitsWithChapter_ReturnsTheChapterScopedPage()
    {
        // Arrange
        var routes = new AccountRoutes(PlatformType.DrunkenKnitwits);

        // Act - Drunken Knitwits builds the segment from the chapter's short name, which is its name with
        // the platform suffix stripped, so "Bristol Drunken Knitwits" addresses as "/bristol".
        var result = routes.Pending(CreateChapter(PlatformType.DrunkenKnitwits, "Bristol Drunken Knitwits"));

        // Assert
        result.Should().Be("/bristol/account/pending");
    }

    [Test]
    public static void Pending_DrunkenKnitwitsWithoutChapter_ReturnsTheSitePage()
    {
        // Arrange
        var routes = new AccountRoutes(PlatformType.DrunkenKnitwits);

        // Act
        var result = routes.Pending(chapter: null);

        // Assert
        result.Should().Be("/account/pending");
    }

    [Test]
    public static void Pending_DefaultPlatformWithChapter_IgnoresTheChapter()
    {
        // Arrange - the chapter must be dropped rather than rendered. No caller reaches this combination
        // today: Group Squirrel only signs members up at the site level and Drunken Knitwits only at the
        // chapter level, so the guard is defensive rather than a fix. It pins the contract for anything
        // that does pass a chapter later - Group Squirrel addresses a chapter as "/groups/{slug}", and
        // there is no /groups/{slug}/account/pending page to land on.
        var routes = new AccountRoutes(PlatformType.Default);

        // Act
        var result = routes.Pending(CreateChapter(PlatformType.Default, "Test Chapter"));

        // Assert
        result.Should().Be("/account/pending");
    }

    private static Chapter CreateChapter(PlatformType platform, string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Platform = platform,
        Slug = "test-chapter"
    };
}
