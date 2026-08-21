using ODK.Core.Chapters;

namespace ODK.Services.Web;

public interface IUrlProvider
{
    string ActivateAccountUrl(Chapter? chapter, string token);

    string BaseUrl();

    string ChapterJoin(Chapter chapter, string inviteToken);

    string ChapterSubscription(Chapter chapter);

    string ConfirmEmailAddressUpdate(Chapter? chapter, string token);

    string ConversationAdminUrl(Chapter chapter, Guid conversationId);

    string ConversationUrl(Chapter chapter, Guid conversationId);

    string EmailPreferences(Chapter? chapter);

    string EventRsvpUrl(Chapter chapter, string shortcode);

    string EventsUrl(Chapter chapter);

    string EventUrl(Chapter chapter, string shortcode);

    string GroupUrl(Chapter chapter);

    string GroupsUrl();

    /// <summary>The public join page, used as the destination in a referral email.</summary>
    string JoinUrl();

    string LoginUrl(Chapter? chapter);

    string MemberAdminUrl(Chapter chapter, Guid memberId);

    string MemberSiteSubscriptionUrl();

    string MessageAdminUrl(Chapter chapter, Guid messageId);

    string MessageSiteAdminUrl(Guid messageId);

    string PasswordReset(Chapter? chapter, string token);

    string SiteAdminGroups();

    /// <summary>The site admin's view of a member's thread with the site.</summary>
    string SiteConversationAdminUrl(Guid conversationId);

    /// <summary>The member's view of their own thread with the site.</summary>
    string SiteConversationUrl(Guid conversationId);

    string TopicApprovalUrl();
}