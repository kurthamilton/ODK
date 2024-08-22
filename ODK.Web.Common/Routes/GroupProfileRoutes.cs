using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public class GroupProfileRoutes
{
    public string Index(Chapter chapter) => $"/groups/{chapter.Slug}/profile";

    public string Subscription(Chapter chapter) => $"{Index(chapter)}/subscription";
}
