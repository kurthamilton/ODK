using System;
using System.Web;
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

    public string Conversations(Chapter chapter) => Conversations(chapter, archived: false);

    public string Conversations(Chapter chapter, bool archived)
    {
        var path = Platform switch
        {
            PlatformType.DrunkenKnitwits => AccountRoutes.Conversations(chapter),
            _ => GroupPath(chapter, "/conversations")
        };

        return $"{path}{(archived ? "?archived=true" : null)}";
    }

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
        => $"{EventCheckout(chapter, shortcode)}/confirm?sessionId={{sessionId}}";

    public string Events(Chapter chapter) => GroupPath(chapter, "/events");

    public string Group(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.ShortName}".ToLowerInvariant(),
        _ => $"{Index()}/{chapter.Slug}".ToLowerInvariant()
    };

    public string Image(Guid chapterId, int version) => $"/groups/{chapterId}/image?v={version}";

    public string Index() => Platform switch
    {
        PlatformType.DrunkenKnitwits => string.Empty,
        _ => "/groups"
    };

    /// <summary>
    /// Where someone who is not yet a member goes to become one.
    /// </summary>
    /// <remarks>
    /// The two platforms reach that through different pages, so this is the one place that knows which. Signing
    /// up on Drunken Knitwits *is* joining the chapter, so there is no separate join page - it is the chapter's
    /// account sign-up. Group Squirrel has members before they have groups, so joining is its own page and
    /// assumes an account already exists.
    /// </remarks>
    public string Join(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => AccountRoutes.Join(chapter),
        _ => GroupPath(chapter, "/join")
    };

    /// <summary>
    /// <see cref="Join(Chapter)"/> carrying an invitation token, so the page can identify who was invited and
    /// fill in what is already known about them.
    /// </summary>
    /// <remarks>
    /// Only Drunken Knitwits can use it: its join page is the chapter's account sign-up and is anonymous, while
    /// Group Squirrel's requires an account already. The token is still appended there so the link is not
    /// silently different between platforms, and the page ignores it until Group Squirrel has a page that can
    /// accept an invitation without one.
    /// </remarks>
    public string Join(Chapter chapter, string inviteToken)
        => $"{Join(chapter)}?token={HttpUtility.UrlEncode(inviteToken)}";

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

    public string SubscriptionConfirm(Chapter chapter)
        => $"{Subscription(chapter)}/confirm?sessionId={{sessionId}}";

    private string GroupPath(Chapter chapter, string path) => $"{Group(chapter)}{path}";
}