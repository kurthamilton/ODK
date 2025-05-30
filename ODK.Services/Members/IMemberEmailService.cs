﻿using ODK.Core.Chapters;
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
    Task SendActivationEmail(Chapter? chapter, Member member, string activationToken);

    Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token);

    Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendChapterConversationEmail(
        Chapter chapter,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply);

    Task SendChapterMessage(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        ChapterContactMessage contactMessage);

    Task<ServiceResult> SendChapterMessageReply(
        Chapter chapter,
        ChapterContactMessage originalMessage,
        string reply);

    Task SendDuplicateMemberEmail(Chapter? chapter, Member member);

    Task SendEventCommentEmail(
        Chapter chapter,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember);

    Task SendEventInvites(
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members);

    Task SendGroupApprovedEmail(Chapter chapter, Member owner);

    Task SendMemberApprovedEmail(Chapter chapter, Member member);

    Task SendIssueReply(
        Issue issue,
        IssueMessage reply,
        Member? toMember,
        SiteEmailSettings siteEmailSettings);

    Task SendMemberChapterSubscriptionConfirmationEmail(
        Chapter chapter,
        ChapterPaymentSettings chapterPaymentSettings,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc);

    Task SendMemberChapterSubscriptionExpiringEmail(
        Chapter chapter, 
        Member member, 
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate);

    Task SendMemberDeleteEmail(Chapter chapter, Member member, string? reason);

    Task SendMemberLeftChapterEmail(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member, 
        string? reason);

    Task SendNewGroupEmail(Chapter chapter, ChapterTexts texts, SiteEmailSettings settings);

    Task SendNewIssueEmail(Member member, Issue issue, IssueMessage message, SiteEmailSettings settings);

    Task SendNewMemberAdminEmail(
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,        
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewMemberEmailsAsync(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);

    Task SendNewTopicEmail(IReadOnlyCollection<INewTopic> newTopics, SiteEmailSettings settings);

    Task SendPasswordResetEmail(Chapter? chapter, Member member, string token);

    Task SendPaymentNotification(Payment payment, Currency currency, SiteEmailSettings settings);

    Task SendSiteMessage(SiteContactMessage message, SiteEmailSettings settings);

    Task<ServiceResult> SendSiteMessageReply(SiteContactMessage originalMessage, string reply);

    Task SendSiteSubscriptionExpiredEmail(Member member);

    Task SendSiteWelcomeEmail(Member member);

    Task<ServiceResult> SendTestEmail(Chapter? chapter, Member to, EmailType type);

    Task SendTopicApprovedEmails(IReadOnlyCollection<INewTopic> newTopics, IReadOnlyCollection<Member> members);

    Task SendTopicRejectedEmails(IReadOnlyCollection<INewTopic> newTopics, IReadOnlyCollection<Member> members);
}
