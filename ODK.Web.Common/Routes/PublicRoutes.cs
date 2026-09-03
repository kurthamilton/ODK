using System.Collections.Generic;
using System.Linq;
using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Core.Platforms;
using ODK.Services.Sitemap.ViewModels;
using ODK.Web.Common.Sitemap;

namespace ODK.Web.Common.Routes;

/// <summary>
/// The pages an anonymous visitor may open, as one registry. This is the single definition of what the
/// public surface contains; the sitemap derives from it, so a new public page is registered once here
/// rather than restated wherever it needs to be enumerated.
/// </summary>
/// <remarks>
/// The paths come from the route accessors alongside this class rather than being written out again, so a
/// route that moves takes its registry entry with it. What the registry adds is the conditions: which
/// platform a page exists on, and what a group has to have configured for a visitor to reach it.
/// </remarks>
public class PublicRoutes
{
    public PublicRoutes(GroupRoutes groupRoutes, SiteRoutes siteRoutes, PlatformType platform)
    {
        GroupRoutes = groupRoutes;
        Platform = platform;
        SiteRoutes = siteRoutes;
    }

    protected PlatformType Platform { get; }

    private GroupRoutes GroupRoutes { get; }

    private SiteRoutes SiteRoutes { get; }

    public PublicRoute Event(Chapter chapter, string shortcode) => new()
    {
        Feature = ChapterFeatureType.Events,
        Path = GroupRoutes.Event(chapter, shortcode)
    };

    /// <summary>
    /// A group's own public pages. The group home page is not gated on the About
    /// <see cref="PageType"/> on either platform: hiding About on Group Squirrel drops it from the group
    /// menu, where it points at the home page, but the home page itself still renders.
    /// </summary>
    public IReadOnlyCollection<PublicRoute> Group(Chapter chapter) =>
    [
        new()
        {
            Path = GroupRoutes.Group(chapter)
        },
        new()
        {
            ChapterPage = PageType.About,
            Path = GroupRoutes.About(chapter),
            Platform = PlatformType.DrunkenKnitwits
        },
        new()
        {
            ChapterPage = PageType.Contact,
            Path = GroupRoutes.Contact(chapter)
        },
        new()
        {
            Path = GroupRoutes.Questions(chapter),
            Platform = PlatformType.Default,
            RequiresQuestions = true
        },
        new()
        {
            Feature = ChapterFeatureType.Events,
            Path = GroupRoutes.Events(chapter)
        },
        new()
        {
            Feature = ChapterFeatureType.Events,
            Path = GroupRoutes.PastEvents(chapter),
            Platform = PlatformType.Default
        }
    ];

    /// <summary>
    /// The pages that belong to the site rather than to a group.
    /// </summary>
    /// <remarks>
    /// Sign-up, sign-in, password reset, invitation and checkout pages are deliberately absent. They are
    /// anonymously reachable but they are steps in a flow rather than destinations, and every one of them
    /// is linked from a page that is here.
    /// </remarks>
    public IReadOnlyCollection<PublicRoute> Site() =>
    [
        new()
        {
            Path = "/"
        },
        new()
        {
            Path = GroupRoutes.Index(),
            Platform = PlatformType.Default
        },
        new()
        {
            Path = SiteRoutes.About,
            Platform = PlatformType.Default
        },
        new()
        {
            Path = SiteRoutes.Contact
        },
        new()
        {
            Path = SiteRoutes.Pricing,
            Platform = PlatformType.Default
        },
        new()
        {
            Path = SiteRoutes.Privacy
        }
    ];

    /// <summary>
    /// The registry resolved against what each group has configured, in the order the sitemap lists it:
    /// the site's own pages, then each group with its pages and then its events.
    /// </summary>
    public IReadOnlyCollection<SitemapNode> Sitemap(SitemapViewModel viewModel)
    {
        var nodes = new List<SitemapNode>();

        foreach (var route in Site().Where(x => x.ExistsOn(Platform)))
        {
            nodes.Add(new SitemapNode
            {
                Path = route.Path
            });
        }

        foreach (var chapter in viewModel.Chapters)
        {
            foreach (var route in Group(chapter.Chapter).Where(x => Includes(x, chapter)))
            {
                nodes.Add(new SitemapNode
                {
                    Path = route.Path
                });
            }

            foreach (var @event in chapter.Events)
            {
                var route = Event(chapter.Chapter, @event.Shortcode);
                if (!Includes(route, chapter))
                {
                    continue;
                }

                nodes.Add(new SitemapNode
                {
                    LastModifiedUtc = @event.PublishedUtc,
                    Path = route.Path
                });
            }
        }

        return nodes;
    }

    private bool Includes(PublicRoute route, SitemapChapterViewModel chapter)
    {
        if (!route.ExistsOn(Platform))
        {
            return false;
        }

        if (route.ChapterPage != null &&
            chapter.ChapterPages.FirstOrDefault(x => x.PageType == route.ChapterPage)?.Hidden == true)
        {
            return false;
        }

        if (route.Feature != null &&
            chapter.PrivacySettings.Visibility(route.Feature.Value) != ChapterFeatureVisibilityType.Public)
        {
            return false;
        }

        return !route.RequiresQuestions || chapter.HasQuestions;
    }
}
