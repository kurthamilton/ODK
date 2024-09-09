using System;
using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class MemberGroupRoutes
{
    public string Event(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{eventId}";

    public string EventAttendees(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{@eventId}/attendees";

    public string EventCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Events(platform, chapter)}/create",
        _ => $"{Events(platform, chapter)}/new"
    };

    public string EventInvites(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{@eventId}/invites";

    public string Events(PlatformType platform, Chapter chapter) => 
        $"{Group(platform, chapter)}/events";

    public string EventSettings(PlatformType platform, Chapter chapter)
        => $"{Events(platform, chapter)}/settings";

    public string EventTickets(PlatformType platform, Chapter chapter, Guid eventId)
        => $"{Events(platform, chapter)}/{@eventId}/tickets";

    public string Group(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"/{chapter.Name.ToLowerInvariant()}/admin",
        _ => $"{Index(platform)}/{chapter.Id}"
    };

    public string GroupConversation(PlatformType platform, Chapter chapter, Guid conversationId)
        => $"{GroupConversations(platform, chapter)}/{conversationId}";

    public string GroupConversations(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/conversations",
        _ => $"{Group(platform, chapter)}/conversations"
    };

    public string GroupConversationsRead(PlatformType platform, Chapter chapter) 
        => $"{GroupConversations(platform, chapter)}/read";

    public string GroupCreate(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => $"{Index(platform)}/new"
    };

    public string GroupMessage(PlatformType platform, Chapter chapter, Guid messageId)
        => $"{GroupMessages(platform, chapter)}/{messageId}";

    public string GroupMessages(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/messages",
        _ => $"{Group(platform, chapter)}/messages"
    };

    public string GroupPayments(PlatformType platform, Chapter chapter)
        => $"{Group(platform, chapter)}/payments";

    public string GroupPrivacy(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/privacy",
        _ => $"{Group(platform, chapter)}/privacy"
    };

    public string GroupProperties(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/properties",
        _ => $"{Group(platform, chapter)}/properties"
    };

    public string GroupProperty(PlatformType platform, Chapter chapter, Guid propertyId)
        => $"{GroupProperties(platform, chapter)}/{propertyId}";

    public string GroupPropertyCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{GroupProperties(platform, chapter)}/create",
        _ => $"{GroupProperties(platform, chapter)}/new"
    };

    public string GroupQuestion(PlatformType platform, Chapter chapter, Guid questionId)
        => $"{GroupQuestions(platform, chapter)}/{questionId}";

    public string GroupQuestionCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{GroupQuestions(platform, chapter)}/create",
        _ => $"{GroupQuestions(platform, chapter)}/new"
    };

    public string GroupQuestions(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/questions",
        _ => $"{Group(platform, chapter)}/questions"
    };

    public string GroupSocialMedia(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/social-media",
        _ => $"{Group(platform, chapter)}/social-media"
    };

    public string GroupTexts(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Group(platform, chapter)}/chapter/text",
        _ => $"{Group(platform, chapter)}/texts"
    };

    public string GroupTopics(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => Group(platform, chapter),
        _ => $"{Group(platform, chapter)}/topics"
    };

    public string Index(PlatformType platform) => platform switch
    {
        PlatformType.DrunkenKnitwits => "/",
        _ => "/my/groups"
    };

    public string Member(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Members(platform, chapter)}/{memberId}";

    public string MemberAdmin(PlatformType platform, Chapter chapter, ChapterAdminMember adminMember)
        => $"{MemberAdmins(platform, chapter)}/{adminMember.MemberId}";

    public string MemberAdmins(PlatformType platform, Chapter chapter) 
        => $"{Members(platform, chapter)}/admins";

    public string MemberConversations(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/conversations";

    public string MemberEmail(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/email";

    public string MemberEvents(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/events";

    public string MemberImage(PlatformType platform, Chapter chapter, Guid memberId)
        => $"{Member(platform, chapter, memberId)}/image";

    public string MembersDownload(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/download";

    public string Members(PlatformType platform, Chapter chapter)
        => $"{Group(platform, chapter)}/members";

    public string MembersEmail(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/email";

    public string MembershipSettings(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/membership";

    public string MembersSubscription(PlatformType platform, Chapter chapter, ChapterSubscription subscription)
        => $"{MembersSubscriptions(platform, chapter)}/{subscription.Id}";

    public string MembersSubscriptionCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{MembersSubscriptions(platform, chapter)}/create",
        _ => $"{MembersSubscriptions(platform, chapter)}/new"
    };

    public string MembersSubscriptions(PlatformType platform, Chapter chapter)
        => $"{Members(platform, chapter)}/subscriptions";

    public string Venue(PlatformType platform, Chapter chapter, Guid venueId)
        => $"{Venues(platform, chapter)}/{venueId}";    

    public string VenueCreate(PlatformType platform, Chapter chapter) => platform switch
    {
        PlatformType.DrunkenKnitwits => $"{Venues(platform, chapter)}/create",
        _ => $"{Venues(platform, chapter)}/new"
    };

    public string VenueEvents(PlatformType platform, Chapter chapter, Guid venueId)
        => $"{Venue(platform, chapter, venueId)}/events";

    public string Venues(PlatformType platform, Chapter chapter)
        => $"{Events(platform, chapter)}/venues";
}
