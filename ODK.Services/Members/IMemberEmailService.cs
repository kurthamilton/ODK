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
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string activationToken);

    Task SendAddressUpdateEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string newEmailAddress,
        string token);

    Task SendBulkEmail(
        ServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendChapterConversationEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply);

    Task SendChapterMessage(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        ChapterContactMessage contactMessage);

    Task<ServiceResult> SendChapterMessageReply(
        ServiceRequest request,
        Chapter chapter,
        ChapterContactMessage originalMessage,
        string reply);

    Task SendDuplicateMemberEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member);

    Task SendEventCommentEmail(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember);

    Task SendEventInvites(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members);

    Task SendGroupApprovedEmail(
        ServiceRequest request,
        Chapter chapter,
        Member owner);

    Task SendMemberApprovedEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member);

    Task SendIssueReply(
        ServiceRequest request,
        Issue issue,
        IssueMessage reply,
        Member? toMember,
        SiteEmailSettings siteEmailSettings);

    Task SendMemberChapterSubscriptionConfirmationEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc);

    Task SendMemberChapterSubscriptionExpiringEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member,
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate);

    Task SendMemberDeleteEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member,
        string? reason);

    Task SendMemberLeftChapterEmail(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        string? reason);

    Task SendNewGroupEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterTexts texts,
        SiteEmailSettings settings);

    Task SendNewIssueEmail(
        ServiceRequest request,
        Member member,
        Issue issue,
        IssueMessage message,
        SiteEmailSettings settings);

    Task SendNewMemberAdminEmail(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewMemberEmailsAsync(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewTopicEmail(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        SiteEmailSettings settings);

    Task SendPasswordResetEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string token);

    Task SendPaymentNotification(
        ServiceRequest request,
        Member member,
        Chapter? chapter,
        Payment payment,
        Currency currency,
        SiteEmailSettings settings);

    Task SendSiteMessage(
        ServiceRequest request,
        SiteContactMessage message,
        SiteEmailSettings settings);

    Task<ServiceResult> SendSiteMessageReply(
        ServiceRequest request,
        SiteContactMessage originalMessage,
        string reply);

    Task SendSiteSubscriptionExpiredEmail(
        ServiceRequest request,
        Member member);

    Task SendSiteWelcomeEmail(
        ServiceRequest request,
        Member member);

    Task<ServiceResult> SendTestEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type);

    Task SendTopicApprovedEmails(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members);

    Task SendTopicRejectedEmails(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members);
}