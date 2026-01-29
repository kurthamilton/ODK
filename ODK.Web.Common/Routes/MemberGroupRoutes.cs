using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class MemberGroupRoutes
{
    public MemberGroupRoutes(PlatformType platform)
    {
        Platform = platform;
    }

    protected PlatformType Platform { get; }

    public string Event(Chapter chapter, Guid eventId)
        => $"{Events(chapter)}/{eventId}";

    public string EventCreate(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Events(chapter)}/create",
        _ => $"{Events(chapter)}/new"
    };

    public string EventInvites(Chapter chapter, Guid eventId)
        => $"{Event(chapter, eventId)}/invites";

    public string EventResponses(Chapter chapter, Guid eventId)
        => $"{Event(chapter, eventId)}/responses";

    public string Events(Chapter chapter) => $"{Group(chapter)}/events";

    public string EventSettings(Chapter chapter) => $"{Events(chapter)}/settings";

    public string EventTickets(Chapter chapter, Guid eventId)
        => $"{Event(chapter, eventId)}/tickets";

    public string Group(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.ShortName.ToLowerInvariant()}/admin",
        _ => $"{Index()}/{chapter.Id}"
    };

    public string GroupConversation(Chapter chapter, Guid conversationId)
        => $"{GroupConversations(chapter)}/{conversationId}";

    public string GroupConversations(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/conversations",
        _ => $"{Group(chapter)}/conversations"
    };

    public string GroupConversationsRead(Chapter chapter) => $"{GroupConversations(chapter)}/read";

    public string GroupCreate() => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Index()}/new"
    };

    public string GroupDelete(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Group(chapter)}/delete"
    };

    public string GroupImage(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Group(chapter)}/image"
    };

    public string GroupLocation(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Group(chapter)}/location"
    };

    public string GroupMessage(Chapter chapter, Guid messageId)
        => $"{GroupMessages(chapter)}/{messageId}";

    public string GroupMessages(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/messages",
        _ => $"{Group(chapter)}/messages"
    };

    public string GroupMessagesSpam(Chapter chapter) => $"{GroupMessages(chapter)}/spam";

    public string GroupPages(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/pages",
        _ => $"{Group(chapter)}/pages"
    };

    public string GroupPayments(Chapter chapter)
            => $"{Group(chapter)}/payments";

    public string GroupPrivacy(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/privacy",
        _ => $"{Group(chapter)}/privacy"
    };

    public string GroupQuestion(Chapter chapter, Guid questionId)
        => $"{GroupQuestions(chapter)}/{questionId}";

    public string GroupQuestionCreate(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{GroupQuestions(chapter)}/create",
        _ => $"{GroupQuestions(chapter)}/new"
    };

    public string GroupQuestions(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/questions",
        _ => $"{Group(chapter)}/questions"
    };

    public string GroupSocialMedia(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/social-media",
        _ => $"{Group(chapter)}/social-media"
    };

    public string GroupSubscription(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/subscription",
        _ => $"{Group(chapter)}/subscription"
    };

    public string GroupSubscriptionCheckout(Chapter chapter, string priceIdPlaceholder) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/subscription/{priceIdPlaceholder}/checkout",
        _ => $"{Group(chapter)}/subscription/{priceIdPlaceholder}/checkout"
    };

    public string GroupTexts(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(chapter)}/chapter/text",
        _ => $"{Group(chapter)}/texts"
    };

    public string GroupTheme(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Group(chapter)}/theme"
    };

    public string GroupTopics(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => Group(chapter),
        _ => $"{Group(chapter)}/topics"
    };

    public string Index() => Platform switch
    {
        // Member groups not implemented in DrunkenKnitwits platform
        PlatformType.DrunkenKnitwits => "/",
        _ => "/my/groups"
    };

    public string Member(Chapter chapter, Guid memberId) => $"{Members(chapter)}/{memberId}";

    public string MemberAdmin(Chapter chapter, ChapterAdminMember adminMember)
        => $"{MemberAdmins(chapter)}/{adminMember.MemberId}";

    public string MemberAdmins(Chapter chapter)
        => $"{Members(chapter)}/admins";

    public string MemberApprovals(Chapter chapter)
        => $"{Members(chapter)}/approvals";

    public string MemberConversations(Chapter chapter, Guid memberId)
        => $"{Member(chapter, memberId)}/conversations";

    public string MemberDelete(Chapter chapter, Guid memberId)
        => $"{Member(chapter, memberId)}/delete";

    public string MemberEvents(Chapter chapter, Guid memberId)
        => $"{Member(chapter, memberId)}/events";

    public string MemberImage(Chapter chapter, Guid memberId)
        => $"{Member(chapter, memberId)}/image";

    public string MemberPayments(Chapter chapter, Guid memberId)
        => $"{Member(chapter, memberId)}/payments";

    public string MemberProperties(Chapter chapter) => $"{Members(chapter)}/properties";

    public string MemberProperty(Chapter chapter, Guid propertyId)
        => $"{MemberProperties(chapter)}/{propertyId}";

    public string MemberPropertyCreate(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{MemberProperties(chapter)}/create",
        _ => $"{MemberProperties(chapter)}/new"
    };

    public string MembersDownload(Chapter chapter)
        => $"/groups/{chapter.Id}/members/download";

    public string Members(Chapter chapter) => $"{Group(chapter)}/members";

    public string MembersEmail(Chapter chapter) => $"{Members(chapter)}/email";

    public string MembershipSettings(Chapter chapter) => $"{Members(chapter)}/membership";

    public string MembersSubscription(Chapter chapter, ChapterSubscription subscription)
        => $"{MembersSubscriptions(chapter)}/{subscription.Id}";

    public string MembersSubscriptionCreate(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{MembersSubscriptions(chapter)}/create",
        _ => $"{MembersSubscriptions(chapter)}/new"
    };

    public string MembersSubscriptions(Chapter chapter) => $"{Members(chapter)}/subscriptions";

    public string Venue(Chapter chapter, Guid venueId) => $"{Venues(chapter)}/{venueId}";

    public string VenueCreate(Chapter chapter) => Platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Venues(chapter)}/create",
        _ => $"{Venues(chapter)}/new"
    };

    public string VenueEvents(Chapter chapter, Guid venueId)
        => $"{Venue(chapter, venueId)}/events";

    public string Venues(Chapter chapter)
        => $"{Events(chapter)}/venues";
}