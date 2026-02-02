using System;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Services.Security;

namespace ODK.Web.Common.Routes;

public class GroupAdminRoutes
{
    public GroupAdminRoutes(PlatformType platform)
    {
        Platform = platform;
    }

    protected PlatformType Platform { get; }

    public GroupAdminRoute AdminMember(Chapter chapter, ChapterAdminMember adminMember)
        => AdminMembers(chapter).Child($"/{adminMember.MemberId}");

    public GroupAdminRoute AdminMembers(Chapter chapter)
        => Members(chapter).Child("/admins", ChapterAdminSecurable.AdminMembers);

    public GroupAdminRoute Conversation(Chapter chapter, Guid conversationId)
        => Conversations(chapter).Child($"/{conversationId}");

    public GroupAdminRoute Conversations(Chapter chapter)
        => Group(chapter).Child("/conversations", ChapterAdminSecurable.Conversations);

    public GroupAdminRoute ConversationsRead(Chapter chapter)
        => Conversations(chapter).Child("/read");

    public GroupAdminRoute Create() => Index().Child("/new");

    public GroupAdminRoute Delete(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/delete", ChapterAdminSecurable.Delete, PlatformType.Default)
    };

    public GroupAdminRoute Email(Chapter chapter, EmailType type) => Emails(chapter).Child($"/{type}");

    public GroupAdminRoute Emails(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits 
            => Group(chapter).Child("/emails", ChapterAdminSecurable.Emails, PlatformType.DrunkenKnitwits),
        _ => GroupAdminRoute.Default
    };

    public GroupAdminRoute Event(Chapter chapter, Guid eventId) 
        => Events(chapter).Child($"/{eventId}");

    public GroupAdminRoute EventCreate(Chapter chapter) 
        => Events(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute EventInvites(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/invites");

    public GroupAdminRoute EventResponses(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/responses");

    public GroupAdminRoute Events(Chapter chapter)
        => Base(chapter).Child("/events", ChapterAdminSecurable.Events);

    public GroupAdminRoute EventSettings(Chapter chapter) 
        => Events(chapter).Child("/settings", ChapterAdminSecurable.EventSettings);

    public GroupAdminRoute EventTickets(Chapter chapter, Guid eventId)
        => Event(chapter, eventId).Child("/tickets");

    public GroupAdminRoute Group(Chapter chapter) => Base(chapter).Child(Platform switch
    {
        PlatformType.DrunkenKnitwits => "/chapter",
        _ => ""
    });

    public GroupAdminRoute Home(Chapter chapter) => Base(chapter);

    public GroupAdminRoute Image(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/image", ChapterAdminSecurable.Branding, PlatformType.Default)
    };

    public GroupAdminRoute Index() => Platform switch
    {
        // Member groups not implemented in DrunkenKnitwits platform
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => new()
        {
            Path = "/my/groups",
            Platform = PlatformType.Default,
            Securable = ChapterAdminSecurable.Any
        }
    };

    public GroupAdminRoute Location(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/location", ChapterAdminSecurable.Location, PlatformType.Default)
    };

    public GroupAdminRoute Member(Chapter chapter, Guid memberId)
        => Members(chapter).Child($"/{memberId}");

    public GroupAdminRoute MemberApprovals(Chapter chapter)
        => Members(chapter).Child("/approvals", ChapterAdminSecurable.MemberApprovals, PlatformType.Default);

    public GroupAdminRoute MemberConversations(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/conversations", ChapterAdminSecurable.Conversations);

    public GroupAdminRoute MemberDelete(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/delete", ChapterAdminSecurable.MemberApprovals);

    public GroupAdminRoute MemberEvents(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/events");

    public GroupAdminRoute MemberImage(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/image", ChapterAdminSecurable.MemberImage, PlatformType.DrunkenKnitwits);

    public GroupAdminRoute MemberPayments(Chapter chapter, Guid memberId)
        => Member(chapter, memberId).Child("/payments", ChapterAdminSecurable.Payments);

    public GroupAdminRoute MemberProperties(Chapter chapter)
        => Members(chapter).Child("/properties", ChapterAdminSecurable.Properties);

    public GroupAdminRoute MemberProperty(Chapter chapter, Guid propertyId)
        => MemberProperties(chapter).Child($"/{propertyId}");

    public GroupAdminRoute MemberPropertyCreate(Chapter chapter)
        => MemberProperties(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute MembersDownload(Chapter chapter)
        => new GroupAdminRoute
        {
            Path = $"/groups/{chapter.Id}/members/download",
            Securable = ChapterAdminSecurable.MemberExport
        };

    public GroupAdminRoute Members(Chapter chapter)
        => Base(chapter).Child("/members", ChapterAdminSecurable.Members);

    public GroupAdminRoute MembersEmail(Chapter chapter) =>
        Members(chapter).Child("/email", ChapterAdminSecurable.BulkEmail);

    public GroupAdminRoute MembershipSettings(Chapter chapter)
        => Members(chapter).Child("/membership", ChapterAdminSecurable.MembershipSettings);

    public GroupAdminRoute MembersSubscription(Chapter chapter, ChapterSubscription subscription)
        => Subscriptions(chapter).Child($"/{subscription.Id}");

    public GroupAdminRoute MembersSubscriptionCreate(Chapter chapter)
        => Subscriptions(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute Message(Chapter chapter, Guid messageId)
        => Messages(chapter).Child($"/{messageId}");

    public GroupAdminRoute Messages(Chapter chapter) 
        => Group(chapter).Child("/messages", ChapterAdminSecurable.ContactMessages);

    public GroupAdminRoute MessagesSpam(Chapter chapter)
        => Messages(chapter).Child("/spam");

    public GroupAdminRoute Pages(Chapter chapter) 
        => Group(chapter).Child("/pages", ChapterAdminSecurable.Pages, PlatformType.Default);

    public GroupAdminRoute PaymentAccount(Chapter chapter)
        => Payments(chapter).Child("/account", ChapterAdminSecurable.PaymentAccount);

    public GroupAdminRoute Payments(Chapter chapter)
        => Base(chapter).Child("/payments", ChapterAdminSecurable.Payments);

    public GroupAdminRoute Privacy(Chapter chapter) 
        => Group(chapter).Child("/privacy", ChapterAdminSecurable.PrivacySettings);

    public GroupAdminRoute Question(Chapter chapter, Guid questionId)
        => Questions(chapter).Child($"/{questionId}");

    public GroupAdminRoute QuestionCreate(Chapter chapter) 
        => Questions(chapter).Child(Platform == PlatformType.DrunkenKnitwits ? "/create" : "/new");

    public GroupAdminRoute Questions(Chapter chapter) 
        => Group(chapter).Child("/questions", ChapterAdminSecurable.Questions);

    public GroupAdminRoute SiteAdmin(Chapter chapter)
        => Base(chapter).Child("/siteadmin");

    public GroupAdminRoute SocialMedia(Chapter chapter) 
        => Group(chapter).Child("/social-media", ChapterAdminSecurable.SocialMedia);

    public GroupAdminRoute Subscription(Chapter chapter) 
        => Group(chapter).Child("/subscription", ChapterAdminSecurable.SiteSubscription, PlatformType.DrunkenKnitwits);

    public GroupAdminRoute SubscriptionCheckout(Chapter chapter, string priceIdPlaceholder) 
        => Subscription(chapter).Child($"/{priceIdPlaceholder}/checkout");

    public GroupAdminRoute Texts(Chapter chapter)
        => Group(chapter).Child("/texts", ChapterAdminSecurable.Texts);

    public GroupAdminRoute Theme(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/theme", ChapterAdminSecurable.Branding, PlatformType.Default)
    };

    public GroupAdminRoute Topics(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => GroupAdminRoute.Default,
        _ => Group(chapter).Child("/topics", ChapterAdminSecurable.Topics, PlatformType.Default)
    };            

    public GroupAdminRoute Subscriptions(Chapter chapter) 
        => Members(chapter).Child("/subscriptions", ChapterAdminSecurable.Subscriptions);

    public GroupAdminRoute Venue(Chapter chapter, Guid venueId) =>
        Venues(chapter).Child($"/{venueId}");

    public GroupAdminRoute VenueCreate(Chapter chapter) => 
        Venues(chapter).Child(Platform switch
        {
            PlatformType.DrunkenKnitwits => "/create",
            _ => "/new"
        });

    public GroupAdminRoute VenueEvents(Chapter chapter, Guid venueId)
        => Venue(chapter, venueId).Child("/events");

    public GroupAdminRoute Venues(Chapter chapter)
        => Events(chapter).Child("/venues", ChapterAdminSecurable.Venues);

    private GroupAdminRoute Base(Chapter chapter) => new()
    {
        Path = Platform switch
        {
            PlatformType.DrunkenKnitwits => $"/{chapter.ShortName.ToLowerInvariant()}/admin",
            _ => $"{Index()}/{chapter.Id}"
        },
        Securable = ChapterAdminSecurable.None
    };
}