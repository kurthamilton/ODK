using System;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Guid eventId) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name}/Events/{eventId}",
        _ => $"{Events(chapter)}/{eventId}"
    };

    public string Events(Chapter chapter) => GroupPath(chapter, "/events");
    public string Group(Chapter chapter) => $"{Index()}/{HttpUtility.UrlEncode(chapter.Slug.ToLowerInvariant())}";
    public string Index() => "/groups";
    public string Join(Chapter chapter) => GroupPath(chapter, "/join");
    public string Members(Chapter chapter) => GroupPath(chapter, "/members");
    public GroupProfileRoutes Profile { get; } = new GroupProfileRoutes();
    public string Questions(Chapter chapter) => GroupPath(chapter, "/faq");
    
    private string GroupPath(Chapter chapter, string path) => $"{Group(chapter)}{path}";
}
