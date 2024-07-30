using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task SendBulkEmail(
        ChapterAdminMember? fromAdminMember,
        Member? fromMember, 
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task SendBulkEmail(Guid currentMemberId, 
        Chapter chapter, 
        IEnumerable<Member> to, 
        string subject, 
        string body);

    Task SendContactEmail(Chapter chapter, string from, string message, 
        IDictionary<string, string> parameters);

    Task SendEventCommentEmail(Chapter chapter, Member? replyToMember, EventComment comment,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(Chapter chapter, EmailAddressee to, EmailType type, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendMemberEmail(Guid currentMemberId, Guid memberId, string subject, string body);

    Task SendNewMemberAdminEmail(Chapter chapter, Member member, 
        IDictionary<string, string> parameters);
}
