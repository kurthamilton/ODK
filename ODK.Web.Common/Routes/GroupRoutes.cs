using System;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{
    public string Contact(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/contact");

    public string Event(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{eventId}";

    public string Events(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/events");

    public string Group(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Index(platform)}/{chapter.Name}".ToLowerInvariant(),
        _ => $"{Index(platform)}/{chapter.Slug}".ToLowerInvariant()
    };

    public string Index(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => "/groups"
    };
    
    public string Join(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/join");
    
    public string Member(PlatformType platform, Chapter chapter, Guid memberId) 
        => $"{Members(platform, chapter)}/{memberId}";

    public string Members(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/members");
    
    public GroupProfileRoutes Profile { get; } = new GroupProfileRoutes();
    
    public string Questions(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/faq");

    private string GroupPath(PlatformType platform, Chapter chapter, string path) 
        => $"{Group(platform, chapter)}{path}";
}
