using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public class GroupProfileRoutes
{
    public string Index(Chapter chapter) => $"/groups/{chapter.Slug}/profile";

    public string Subscription(Chapter chapter) => $"{Index(chapter)}/subscription";

    public string SubscriptionCheckout(Chapter chapter, ChapterSubscription subscription) 
        => $"{Index(chapter)}/subscription/{subscription.Id}/checkout";
}
