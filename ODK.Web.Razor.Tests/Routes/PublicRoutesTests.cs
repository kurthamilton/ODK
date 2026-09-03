using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Web.Common.Routes;
using ODK.Web.Razor.Tests.Views;

namespace ODK.Web.Razor.Tests.Routes;

/// <summary>
/// Holds <see cref="PublicRoutes"/> to the pages that exist, in both directions: every page is either in
/// the registry or explicitly not public, and every route the registry produces reaches a real page.
/// </summary>
/// <remarks>
/// The registry is the only enumeration of the site's public surface and the sitemap is its only consumer,
/// so nothing else would notice a page missing from it. A page reaches this test by existing, so a new one
/// has to be classified before the build is green.
/// </remarks>
[Parallelizable]
public static class PublicRoutesTests
{
    /* Route templates the registry deliberately does not contain, by prefix, with what the prefix denotes.
       By prefix rather than by page because these are whole areas of the site rather than judgement calls:
       a new admin page needs no decision about whether a crawler should see it. */
    private static readonly (string Prefix, string Reason)[] NotPublicPrefixes =
    [
        ("/account/", "a member's own account"),
        ("/{chaptername}/account/", "a member's own account"),
        ("/{chaptername}/admin/", "group admin, behind a chapter securable"),
        ("/my/groups", "group admin, behind a chapter securable"),
        ("/siteadmin/", "site admin"),
        ("/error/", "an error page"),
        ("/{chaptername}/error/", "an error page"),
        ("/groups/{slug}/error/", "an error page"),
        ("/sitemap", "unlisted")
    ];

    /* And the individual ones, which are judgement calls and get a reason each. */
    private static readonly (string Route, string Reason)[] NotPublicRoutes =
    [
        ("/account", "a member's own account"),
        ("/login", "redirects to the sign-in page"),
        ("/{chaptername}/account", "a member's own account"),
        ("/{chaptername}/admin", "group admin, behind a chapter securable"),
        ("/{chaptername}/account/join", "a step in signing up"),
        ("/groups/{slug}/join", "a step in joining a group, and needs an account already"),
        ("/groups/{slug}/accept-invite", "reached from an invitation link, with its token"),
        ("/{chaptername}/events/{shortcode}/checkout", "a step in buying a ticket"),
        ("/{chaptername}/events/{shortcode}/checkout/confirm", "a step in buying a ticket"),
        ("/groups/{slug}/events/{shortcode}/checkout", "a step in buying a ticket"),
        ("/groups/{slug}/events/{shortcode}/checkout/confirm", "a step in buying a ticket"),
        ("/groups/{slug}/subscription", "a member's own membership of a group"),
        ("/groups/{slug}/subscription/confirm", "a step in paying for a membership"),
        ("/groups/{slug}/subscription/{chaptersubscriptionid}/checkout", "a step in paying for a membership"),
        ("/{chaptername}/members", "members only"),
        ("/{chaptername}/members/{id}", "members only"),
        ("/groups/{slug}/members", "members only"),
        ("/groups/{slug}/members/{id}", "members only"),
        ("/groups/{slug}/profile", "a member's own profile in a group"),
        ("/groups/{slug}/conversations", "a member's own conversations with a group"),
        ("/groups/{slug}/conversations/{id}", "a member's own conversations with a group"),
        ("/groups/{chapterid}/subscription-alert", "a fragment loaded into a page"),
        ("/conversations", "a member's own conversations with the site"),
        ("/conversations/{id}", "a member's own conversations with the site"),
        ("/notifications", "a fragment loaded into a page"),
        ("/feedback", "a fragment loaded into a page"),
        ("/tasks", "a fragment loaded into a page"),
        ("/refer", "signed-in members only"),
        ("/sitemap.xml", "the sitemap itself"),
        ("/test", "exists to raise an exception")
    ];

    private static readonly Regex PageDirective = new("@page\\s+\"([^\"]+)\"");

    /* A route template's parameter and everything the template constrains it with, e.g. {id:guid} or
       {chapterName:regex(^[A-Za-z-]+$)}. No constraint in use contains a closing brace. */
    private static readonly Regex Parameter = new(@"\{([A-Za-z_]\w*)[^}]*\}");

    [Test]
    public static void Registry_CoversEveryPageThatIsNotExcluded()
    {
        // Arrange
        var pageRoutes = PageRoutes();
        var registered = RegisteredRoutes();

        // Act
        var unclassified = pageRoutes
            .Where(x => !registered.Contains(x) && !IsExcluded(x))
            .ToArray();

        // Assert
        unclassified.Should().BeEmpty(
            "a page is either public - registered in PublicRoutes, so a crawler is told about it - or "
            + "deliberately not, in which case say so in this test with the reason");
    }

    [Test]
    public static void Registry_ExcludesNothingThatIsRegistered()
    {
        // Arrange
        var registered = RegisteredRoutes();

        // Act
        var both = registered
            .Where(IsExcluded)
            .ToArray();

        // Assert
        both.Should().BeEmpty("a route cannot be both public and excluded - one of the two is stale");
    }

    [Test]
    public static void Registry_ExcludesOnlyPagesThatExist()
    {
        // Arrange
        var pageRoutes = PageRoutes();

        // Act
        var withoutAPage = NotPublicRoutes
            .Select(x => x.Route)
            .Where(x => !pageRoutes.Contains(x))
            .ToArray();

        // Assert
        withoutAPage.Should().BeEmpty("an exclusion for a page that no longer exists is dead weight");
    }

    [Test]
    public static void Registry_ProducesOnlyRoutesThatHaveAPage()
    {
        // Arrange
        var pageRoutes = PageRoutes();

        // Act
        var withoutAPage = RegisteredRoutes()
            .Where(x => !pageRoutes.Contains(x))
            .ToArray();

        // Assert
        withoutAPage.Should().BeEmpty(
            "every registered route is offered to crawlers, so one with no page behind it is a 404 for "
            + "every group on the platform");
    }

    private static Chapter CreateChapter(PlatformType platform) => new()
    {
        Id = Guid.NewGuid(),
        Name = "{chapterName}",
        Platform = platform,
        Slug = "{slug}"
    };

    private static bool IsExcluded(string route)
        => NotPublicPrefixes.Any(x => route.StartsWith(x.Prefix, StringComparison.Ordinal))
            || NotPublicRoutes.Any(x => x.Route == route);

    private static string Normalise(string route)
        => Parameter.Replace(route, x => $"{{{x.Groups[1].Value}}}").ToLowerInvariant();

    /// <summary>
    /// Every page's route template, lowercased and stripped of route constraints, so the templates the
    /// registry produces compare against them directly.
    /// </summary>
    private static IReadOnlySet<string> PageRoutes()
    {
        var pagesDirectory = Path.Combine(ViewFiles.ProjectDirectory(), "Pages");

        var routes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in ViewFiles.All(pagesDirectory))
        {
            var match = PageDirective.Match(File.ReadAllText(file));
            if (match.Success)
            {
                routes.Add(Normalise(match.Groups[1].Value));
            }
        }

        routes.Should().NotBeEmpty("the test reads the web project's own pages, so it must find some");

        return routes;
    }

    /// <summary>
    /// Every route the registry holds, on both platforms, as a route template.
    /// </summary>
    /// <remarks>
    /// The chapter and the event supply their own route parameters as their values: a public route is
    /// string concatenation over a chapter's short name or slug and an event's shortcode, so a chapter
    /// named "{chapterName}" produces the template its pages declare.
    /// </remarks>
    private static IReadOnlySet<string> RegisteredRoutes()
    {
        var routes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var platform in new[] { PlatformType.Default, PlatformType.DrunkenKnitwits })
        {
            var publicRoutes = new OdkRoutes(platform).Public;
            var chapter = CreateChapter(platform);

            var chapterRoutes = publicRoutes
                .Group(chapter)
                .Append(publicRoutes.Event(chapter, "{shortcode}"));

            foreach (var route in publicRoutes.Site().Concat(chapterRoutes))
            {
                if (route.ExistsOn(platform))
                {
                    routes.Add(Normalise(route.Path));
                }
            }
        }

        return routes;
    }
}
