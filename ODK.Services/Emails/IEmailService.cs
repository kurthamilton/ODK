using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task SendBulkEmail(
        ChapterAdminMember? fromAdminMember,
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task SendBulkEmail(
        ChapterAdminMember fromAdminMember,
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

    Task SendContactEmail(SiteContactMessage message);

    Task SendContactEmail(Chapter chapter, ChapterContactMessage message);

    Task SendEventCommentEmail(Chapter chapter, Member? replyToMember, EventComment comment,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, EmailType type, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, EmailType type,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, string subject, string body);

    Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, string subject, string body, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendMemberEmail(Chapter? chapter, ChapterAdminMember? fromAdminMember, EmailAddressee to, string subject, string body);

    Task<ServiceResult> SendMemberEmail(Chapter? chapter, ChapterAdminMember? fromAdminMember, EmailAddressee to, string subject, string body,
        IDictionary<string, string> parameters);

    Task SendNewChapterMemberEmail(Chapter chapter, Member member);

    Task SendNewMemberAdminEmail(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member, 
        IDictionary<string, string> parameters);

    Task SendNewMemberAdminEmail(
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties);
}
