using ODK.Core.Chapters;

namespace ODK.Services.Web;

/// <summary>
/// Absolute URLs for work whose output is read elsewhere - an email, a payment provider's record of a group.
/// Each is built against the platform that owns what the URL is about, so a method taking a
/// <see cref="Chapter"/> follows the group and one taking none follows the platform serving the request.
/// </summary>
public interface IUrlProvider
{
    /// <summary>
    /// The page an invite link lands on, which differs by platform - see
    /// <c>GroupRoutes.AcceptInvite</c>.
    /// </summary>
    string AcceptInviteUrl(Chapter chapter, string inviteToken);

    string ActivateAccountUrl(Chapter? chapter, string token);

    /// <summary>
    /// The site's own address: the group's platform where the URL is about one, otherwise the platform
    /// serving the request.
    /// </summary>
    string BaseUrl(Chapter? chapter);

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

    string RefuseInviteUrl(Chapter chapter, string inviteToken);

    string SiteAdminGroups();

    /// <summary>The site admin's view of a member's thread with the site.</summary>
    string SiteConversationAdminUrl(Guid conversationId);

    /// <summary>The member's view of their own thread with the site.</summary>
    string SiteConversationUrl(Guid conversationId);

    string TopicApprovalUrl();
}