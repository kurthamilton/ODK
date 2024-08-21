using System.Web;
using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{
    public string Events(Chapter chapter) => GroupPath(chapter, "/events");
    public string Group(Chapter chapter) => $"{Index()}/{HttpUtility.UrlEncode(chapter.Slug.ToLowerInvariant())}";
    public string Index() => "/groups";
    public string Join(Chapter chapter) => GroupPath(chapter, "/join");
    public string Members(Chapter chapter) => GroupPath(chapter, "/members");
    public string Profile(Chapter chapter) => GroupPath(chapter, "/profile");
    public string Questions(Chapter chapter) => GroupPath(chapter, "/faq");

    private string GroupPath(Chapter chapter, string path) => $"{Group(chapter)}{path}";
}
