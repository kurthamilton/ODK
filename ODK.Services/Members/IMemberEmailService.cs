using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Payments;
using ODK.Core.Topics;
using ODK.Core.Venues;

namespace ODK.Services.Members;

public interface IMemberEmailService
{
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

    Task SendMemberApprovedEmail(
        IChapterServiceRequest request,
        Member member);

    Task SendIssueReply(
        IServiceRequest request,
        Issue issue,
        IssueMessage reply,
        Member? toMember,
        SiteEmailSettings siteEmailSettings);

    Task SendMemberChapterSubscriptionConfirmationEmail(
        IChapterServiceRequest request,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc);

    Task SendMemberChapterSubscriptionExpiringEmail(
        IChapterServiceRequest request,
        Member member,
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate);

    Task SendMemberDeleteEmail(
        IChapterServiceRequest request,
        Member member,
        string? reason);

    Task SendMemberLeftChapterEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        string? reason);

    Task SendNewGroupEmail(
        IServiceRequest request,
        ChapterTexts texts,
        SiteEmailSettings settings);

    Task SendNewIssueEmail(
        IServiceRequest request,
        Member member,
        Issue issue,
        IssueMessage message,
        SiteEmailSettings settings);

    Task SendNewMemberAdminEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewMemberEmailsAsync(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewTopicEmail(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        SiteEmailSettings settings);

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
        Currency currency,
        SiteEmailSettings settings);

    Task SendSiteMessage(
        IServiceRequest request,
        SiteContactMessage message,
        SiteEmailSettings settings);

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