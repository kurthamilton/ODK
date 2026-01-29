using ODK.Core.Chapters;

namespace ODK.Web.Common.Routes;

public class GroupProfileRoutes
{
    public GroupProfileRoutes(Chapter chapter)
    {
        Chapter = chapter;
    }

    protected Chapter Chapter { get; }

    public string Index() => $"/groups/{Chapter.Slug}/profile";

    public string Subscription() => $"{Index()}/subscription";

    public string SubscriptionCheckout(ChapterSubscription subscription)
        => $"{Index()}/subscription/{subscription.Id}/checkout";
}
