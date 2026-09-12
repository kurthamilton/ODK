using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Payments;
using ODK.Core.Topics;
using ODK.Core.Venues;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public interface IMemberEmailService
{
    /// <summary>
    /// Renders an email against the same stand-in values a test send uses, without queuing anything, so an
    /// admin can see wording that has not been saved.
    /// </summary>
    Task<RenderedEmail> RenderTestEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type,
        string subject,
        string body);

    Task SendActivationEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string activationToken);

    Task SendAddressUpdateEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string newEmailAddress,
        string token);

    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendChapterConversationEmail(
        IChapterServiceRequest request,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply);

    Task SendChapterMessage(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        ChapterContactMessage contactMessage);

    Task<ServiceResult> SendChapterMessageReply(
        IChapterServiceRequest request,
        ChapterContactMessage originalMessage,
        string reply);

    Task SendDuplicateMemberEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member);

    Task SendEventCommentEmail(
        IChapterServiceRequest request,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember);

    Task SendEventInvites(
        IChapterServiceRequest request,
        Event @event,
        Venue venue,
        IEnumerable<Member> members);

    Task SendEventWaitlistPromotionNotification(
        IChapterServiceRequest request,
        Event @event,
        IEnumerable<Member> members);

    Task SendGroupApprovedEmail(
        IChapterServiceRequest request,
        Member owner);

    /// <summary>
    /// Tells the group's owner that invites it raised are waiting to be emailed, and where to send them
    /// from. An email rather than a notification because the point is to reach them off the platform: a
    /// notification is only read once they next sign in, which is the trip this is meant to prompt.
    /// </summary>
    Task SendInvitesWaitingEmail(
        IChapterServiceRequest request,
        Member owner,
        int count);

    Task SendMemberApprovedEmail(
        IChapterServiceRequest request,
        Member member);

    Task SendMemberChapterSubscriptionConfirmationEmail(
        IChapterServiceRequest request,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc);

    Task SendMemberChapterSubscriptionExpiringEmail(
        IChapterServiceRequest request,
        Member member,
        MemberChapterSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate);

    Task SendMemberDeleteEmail(
        IChapterServiceRequest request,
        Member member,
        string? reason);

    Task SendMemberImportInviteEmail(
        IChapterServiceRequest request,
        Member member,
        string inviteToken);

    Task SendMemberLeftChapterEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        string? reason);

    /// <summary>
    /// Tells site admins a group has been created. <paramref name="chapter"/> is what the email is about,
    /// not what it is sent as - it goes out as the site.
    /// </summary>
    Task SendNewGroupEmail(
        IServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> siteAdmins);

    Task SendNewMemberAdminEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewMemberEmails(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewTopicEmail(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IEnumerable<Member> siteAdmins);

    Task SendPasswordResetEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string token);

    Task SendPaymentNotification(
        IServiceRequest request,
        Member member,
        Chapter? chapter,
        Payment payment,
        Currency currency);

    Task SendSiteConversationEmail(
        IServiceRequest request,
        SiteConversation conversation,
        SiteConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply);

    Task SendSiteMessage(
        IServiceRequest request,
        SiteContactMessage message,
        IEnumerable<Member> siteAdmins);

    Task<ServiceResult> SendSiteMessageReply(
        IServiceRequest request,
        SiteContactMessage originalMessage,
        string reply);

    Task SendSiteSubscriptionExpiredEmail(
        IServiceRequest request,
        Member member);

    Task SendSiteWelcomeEmail(
        IServiceRequest request,
        Member member);

    Task<ServiceResult> SendTestEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type);

    Task SendTopicApprovedEmails(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members);

    Task SendTopicRejectedEmails(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members);
}