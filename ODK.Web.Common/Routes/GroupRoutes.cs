using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class GroupRoutes
{
    public GroupRoutes(AccountRoutes accountRoutes, PlatformType platform)
    {
        AccountRoutes = accountRoutes;
        Platform = platform;
    }

    protected PlatformType Platform { get; }

    private AccountRoutes AccountRoutes { get; }

    public string About(Chapter chapter) => GroupPath(chapter, "/about");

    public string Contact(Chapter chapter) => GroupPath(chapter, "/contact");

    public string Conversation(Chapter chapter, Guid conversationId)
        => $"{Conversations(chapter)}/{conversationId}";

    public string Conversations(Chapter chapter) => $"{Group(chapter)}/conversations";

    public string Error(Chapter chapter, int statusCode)
        => $"{Group(chapter)}/error/{statusCode}";

    public string Event(Chapter chapter, string shortcode) => $"{Events(chapter)}/{shortcode}";

    public string EventLegacy(Chapter chapter, Guid eventId) => $"{Events(chapter)}/{eventId}";

    public string EventAttend(Chapter chapter, string shortcode) => $"{Event(chapter, shortcode)}/rsvp";

    public string EventAttendLegacy(Chapter chapter, Guid eventId)
        => $"{EventLegacy(chapter, eventId)}/rsvp";

    public string EventCheckout(Chapter chapter, string shortcode)
        => $"{Event(chapter, shortcode)}/checkout";

    public string EventCheckoutConfirm(Chapter chapter, string shortcode)
        => $"{EventCheckout(chapter, shortcode)}/confirm";

    public string Events(Chapter chapter) => GroupPath(chapter, "/events");

    public string Group(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.ShortName}".ToLowerInvariant(),
        _ => $"{Index()}/{chapter.Slug}".ToLowerInvariant()
    };

    public string Image(Guid chapterId) => $"/groups/{chapterId}/image";

    public string Index() => Platform switch
    {
        PlatformType.DrunkenKnitwits => string.Empty,
        _ => "/groups"
    };

    public string Join(Chapter chapter) => GroupPath(chapter, "/join");

    public string Member(Chapter chapter, Guid memberId)
        => $"{Members(chapter)}/{memberId}";

    public string Members(Chapter chapter) => GroupPath(chapter, "/members");

    public string PastEvents(Chapter chapter) => $"{Events(chapter)}/past";

    public string Profile(Chapter chapter) => GroupPath(chapter, "/profile");

    public string Questions(Chapter chapter) => GroupPath(chapter, "/faq");

    public string Subscription(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => AccountRoutes.Subscription(chapter),
        _ => GroupPath(chapter, "/subscription")
    };

    public string SubscriptionCheckout(Chapter chapter, ChapterSubscription subscription)
        => $"{Subscription(chapter)}/{subscription.Id}/checkout";

    private string GroupPath(Chapter chapter, string path) => $"{Group(chapter)}{path}";
}