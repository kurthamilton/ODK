using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Tests.Routes;

[Parallelizable]
public static class GroupAdminRoutesTests
{
    [Test]
    public static void LandingRoute_NotAnAdmin_ReturnsNull()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);
        var chapter = CreateChapter();

        // Act
        var result = routes.LandingRoute(chapter, adminMember: null, CreateMember());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void LandingRoute_Organiser_PrefersEvents()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);
        var chapter = CreateChapter();

        // Act
        var result = routes.LandingRoute(chapter, CreateAdminMember(ChapterAdminRole.Organiser), CreateMember());

        // Assert
        result!.Path.Should().Be(routes.Events(chapter).Path);
    }

    [Test]
    public static void LandingRoute_Organiser_ReturnsRouteTheyCanOpen()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);
        var chapter = CreateChapter();
        var adminMember = CreateAdminMember(ChapterAdminRole.Organiser);
        var currentMember = CreateMember();

        // Act
        var result = routes.LandingRoute(chapter, adminMember, currentMember);

        // Assert
        result.Should().NotBeNull();
        result.IsPermitted(adminMember, currentMember, PlatformType.Default).Should().BeTrue();
    }

    [Test]
    public static void LandingRoute_SiteAdminWithNoChapterRole_ReturnsRoute()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);
        var chapter = CreateChapter();

        // Act
        var result = routes.LandingRoute(chapter, adminMember: null, CreateMember(siteAdmin: true));

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public static void PermittedNavigation_NotAnAdmin_ReturnsEmpty()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);

        // Act
        var result = routes.PermittedNavigation(CreateChapter(), adminMember: null, CreateMember());

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public static void PermittedNavigation_NotSiteAdmin_ExcludesSiteAdminSection()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.DrunkenKnitwits);

        // Act
        var result = routes.PermittedNavigation(
            CreateChapter(), CreateAdminMember(ChapterAdminRole.Owner), CreateMember());

        // Assert
        result.Should().NotContain(x => x.RequiresSiteAdmin);
    }

    [Test]
    public static void PermittedNavigation_Organiser_ExcludesOwnerOnlyRoutes()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.Default);
        var chapter = CreateChapter();

        // Act
        var result = routes.PermittedNavigation(
            chapter, CreateAdminMember(ChapterAdminRole.Organiser), CreateMember());

        // Assert
        var paths = result.SelectMany(x => x.Items).Select(x => x.Route.Path).ToArray();
        paths.Should().NotContain(routes.Delete(chapter).Path);
        paths.Should().NotContain(routes.PaymentAccount(chapter).Path);
    }

    [Test]
    public static void PermittedNavigation_SiteAdmin_IncludesSiteAdminSection()
    {
        // Arrange
        var routes = new GroupAdminRoutes(PlatformType.DrunkenKnitwits);

        // Act
        var result = routes.PermittedNavigation(
            CreateChapter(), adminMember: null, CreateMember(siteAdmin: true));

        // Assert
        result.Should().Contain(x => x.RequiresSiteAdmin);
    }

    // ChapterAdminMember.HasAccessTo resolves the site admin flag through its own Member navigation
    // property, which EF populates in production, so it has to be set here too.
    private static ChapterAdminMember CreateAdminMember(ChapterAdminRole role) => new()
    {
        Member = CreateMember(),
        Role = role
    };

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Chapter",
        Slug = "test-chapter"
    };

    private static Member CreateMember(bool siteAdmin = false) => new()
    {
        SiteAdmin = siteAdmin,
        TimeZone = TimeZoneInfo.Utc
    };
}
