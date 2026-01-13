using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{
    public string About(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/about");

    public string Contact(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/contact");

    public string Conversation(PlatformType platform, Chapter chapter, Guid conversationId)
        => $"{Conversations(platform, chapter)}/{conversationId}";

    public string Conversations(PlatformType platform, Chapter chapter)
        => $"{Group(platform, chapter)}/conversations";

    public string Event(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{eventId}";

    public string EventCheckout(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Event(platform, chapter, eventId)}/checkout";

    public string Events(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/events");

    public string Group(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Index(platform)}/{chapter.GetDisplayName(platform)}".ToLowerInvariant(),
        _ => $"{Index(platform)}/{chapter.Slug}".ToLowerInvariant()
    };

    public string Image(Guid chapterId) => $"/groups/{chapterId}/image";

    public string Index(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => string.Empty,
        _ => "/groups"
    };

    public string Join(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/join");

    public string Member(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Members(platform, chapter)}/{memberId}";

    public string Members(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/members");

    public string PastEvents(PlatformType platform, Chapter chapter) => $"{Events(platform, chapter)}/past";

    public string Profile(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/profile");

    public string Questions(PlatformType platform, Chapter chapter) => GroupPath(platform, chapter, "/faq");

    public string Subscription(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => OdkRoutes.Account.Subscription(platform, chapter),
        _ => GroupPath(platform, chapter, "/subscription")
    };

    public string SubscriptionCheckout(PlatformType platform, Chapter chapter, ChapterSubscription subscription)
        => $"{Subscription(platform, chapter)}/{subscription.Id}/checkout";

    private string GroupPath(PlatformType platform, Chapter chapter, string path)
        => $"{Group(platform, chapter)}{path}";
}
