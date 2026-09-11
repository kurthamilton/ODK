using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Platforms;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class UrlProviderTests
{
    private const string DefaultBaseUrl = "https://groupsquirrel.example.com";

    private const string DrunkenKnitwitsBaseUrl = "https://drunkenknitwits.example.com";

    [Test]
    public static void GroupUrl_GroupOnAnotherPlatform_UsesThatPlatformsHostAndRoutes()
    {
        /* Arrange - a Drunken Knitwits group, from a request served as Group Squirrel, which sees every
           platform's groups. A link to the group has to reach the site that hosts it, so both the host and
           the path shape come from the group's platform. */
        var provider = CreateProvider(PlatformType.GroupSquirrel);
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits);

        // Act
        var result = provider.GroupUrl(chapter);

        // Assert
        result.Should().Be($"{DrunkenKnitwitsBaseUrl}/bristol");
    }

    [Test]
    public static void GroupUrl_GroupOnTheServedPlatform_UsesTheServedPlatform()
    {
        // Arrange
        var provider = CreateProvider(PlatformType.GroupSquirrel);
        var chapter = CreateChapter(PlatformType.GroupSquirrel);

        // Act
        var result = provider.GroupUrl(chapter);

        // Assert
        result.Should().Be($"{DefaultBaseUrl}/groups/bristol");
    }

    [Test]
    public static void SiteAdminGroups_AboutTheSiteRatherThanAGroup_UsesTheServedPlatform()
    {
        // Arrange - a URL naming no group is about the site the reader is already on, so it stays there
        // even on a deployment that can see the other platform's groups.
        var provider = CreateProvider(PlatformType.GroupSquirrel);

        // Act
        var result = provider.SiteAdminGroups();

        // Assert
        result.Should().StartWith(DefaultBaseUrl);
    }

    [Test]
    public static void SiteAdminGroups_ProviderGivenAnotherPlatform_UsesThatPlatform()
    {
        /* Arrange - the email pipeline builds against the platform the email is about, not the request's, so
           a URL naming no group has to land on the same site as the ones that do. Otherwise an email
           addressed as Drunken Knitwits would carry a Group Squirrel link the moment it mentioned anything
           other than the group. */
        var provider = CreateProvider(PlatformType.DrunkenKnitwits);

        // Act
        var result = provider.SiteAdminGroups();

        // Assert
        result.Should().StartWith(DrunkenKnitwitsBaseUrl);
    }

    [Test]
    public static void BaseUrl_NoGroup_UsesTheServedPlatform()
    {
        // Arrange
        var provider = CreateProvider(PlatformType.GroupSquirrel);

        // Act
        var result = provider.BaseUrl(chapter: null);

        // Assert
        result.Should().Be(DefaultBaseUrl);
    }

    [Test]
    public static void BaseUrl_GroupOnAnotherPlatform_UsesThatPlatform()
    {
        // Arrange - the platform URL an email carries names the site the email is about, not the one that
        // happened to send it.
        var provider = CreateProvider(PlatformType.GroupSquirrel);
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits);

        // Act
        var result = provider.BaseUrl(chapter);

        // Assert
        result.Should().Be(DrunkenKnitwitsBaseUrl);
    }

    private static Chapter CreateChapter(PlatformType platform) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Bristol",
        Platform = platform,
        Slug = "bristol"
    };

    /// <summary>
    /// The real routes factory and platform provider rather than doubles: what is under test is which
    /// platform each URL is built against, so faking either would only restate the answer.
    /// </summary>
    private static UrlProvider CreateProvider(PlatformType servedPlatform) => new(
        servedPlatform,
        new OdkRoutesFactory(),
        new PlatformProvider(new PlatformProviderSettings
        {
            BaseUrls = new Dictionary<PlatformType, string>
            {
                { PlatformType.GroupSquirrel, DefaultBaseUrl },
                { PlatformType.DrunkenKnitwits, DrunkenKnitwitsBaseUrl }
            },
            Names = new Dictionary<PlatformType, string>
            {
                { PlatformType.GroupSquirrel, "Group Squirrel" },
                { PlatformType.DrunkenKnitwits, "Drunken Knitwits" }
            },
            Platform = servedPlatform
        }));
}
