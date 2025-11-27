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
using ODK.Core.Web;

namespace ODK.Services.Members;

public interface IMemberEmailService
{
    Task SendActivationEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter? chapter, 
        Member member, 
        string activationToken);

    Task SendAddressUpdateEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter? chapter, 
        Member member, 
        string newEmailAddress, 
        string token);

    Task SendBulkEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendChapterConversationEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply);

    Task SendChapterMessage(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        ChapterContactMessage contactMessage);

    Task<ServiceResult> SendChapterMessageReply(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        ChapterContactMessage originalMessage,
        string reply);

    Task SendDuplicateMemberEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter? chapter, 
        Member member);

    Task SendEventCommentEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember);

    Task SendEventInvites(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members);

    Task SendGroupApprovedEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter chapter, 
        Member owner);

    Task SendMemberApprovedEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter chapter, 
        Member member);

    Task SendIssueReply(
        IHttpRequestContext httpRequestContext,
        Issue issue,
        IssueMessage reply,
        Member? toMember,
        SiteEmailSettings siteEmailSettings);

    Task SendMemberChapterSubscriptionConfirmationEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        ChapterPaymentSettings chapterPaymentSettings,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc);

    Task SendMemberChapterSubscriptionExpiringEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        Member member, 
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate);

    Task SendMemberDeleteEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter chapter, 
        Member member, 
        string? reason);

    Task SendMemberLeftChapterEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member, 
        string? reason);

    Task SendNewGroupEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter chapter, 
        ChapterTexts texts, 
        SiteEmailSettings settings);

    Task SendNewIssueEmail(
        IHttpRequestContext httpRequestContext,
        Member member, 
        Issue issue, 
        IssueMessage message, 
        SiteEmailSettings settings);

    Task SendNewMemberAdminEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,        
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewMemberEmailsAsync(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewTopicEmail(
        IHttpRequestContext httpRequestContext, 
        IReadOnlyCollection<INewTopic> newTopics, 
        SiteEmailSettings settings);

    Task SendPasswordResetEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter? chapter, 
        Member member, 
        string token);

    Task SendPaymentNotification(
        IHttpRequestContext httpRequestContext, 
        Payment payment, 
        Currency currency, 
        SiteEmailSettings settings);

    Task SendSiteMessage(
        IHttpRequestContext httpRequestContext, 
        SiteContactMessage message, 
        SiteEmailSettings settings);

    Task<ServiceResult> SendSiteMessageReply(
        IHttpRequestContext httpRequestContext, 
        SiteContactMessage originalMessage, 
        string reply);

    Task SendSiteSubscriptionExpiredEmail(
        IHttpRequestContext httpRequestContext, 
        Member member);

    Task SendSiteWelcomeEmail(
        IHttpRequestContext httpRequestContext, 
        Member member);

    Task<ServiceResult> SendTestEmail(
        IHttpRequestContext httpRequestContext, 
        Chapter? chapter, 
        Member to, 
        EmailType type);

    Task SendTopicApprovedEmails(
        IHttpRequestContext httpRequestContext, 
        IReadOnlyCollection<INewTopic> newTopics, 
        IReadOnlyCollection<Member> members);

    Task SendTopicRejectedEmails(
        IHttpRequestContext httpRequestContext, 
        IReadOnlyCollection<INewTopic> newTopics, 
        IReadOnlyCollection<Member> members);
}
