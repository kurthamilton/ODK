using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IMailProvider _mailProvider;
    private readonly IUnitOfWork _unitOfWork;

    public EmailService(
        IUnitOfWork unitOfWork,
        IMailProvider mailProvider)
    {
        _mailProvider = mailProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        EmailType type,
        IDictionary<string, string> parameters)
    {
        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Parameters = parameters,
            Subject = "",
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
            Type = type
        });
    }

    public async Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
        });
    }    

    public async Task SendEventCommentEmail(
        Chapter chapter, 
        Member? replyToMember, 
        EventComment comment,
        IDictionary<string, string> parameters)
    {
        var (chapterAdminMembers, replyToMemberEmailPreference) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => replyToMember != null
                ? x.MemberEmailPreferenceRepository.GetByMemberId(replyToMember.Id, MemberEmailPreferenceType.EventMessages)
                : new DefaultDeferredQuerySingleOrDefault<MemberEmailPreference>());

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveEventCommentEmails));
        if (replyToMember != null && replyToMemberEmailPreference?.Disabled != true)
        {
            to = to.Append(replyToMember.ToEmailAddressee());
        }

        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Parameters = parameters,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.EventComment
        });
    }

    public Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, EmailType type,
        IDictionary<string, string> parameters)
        => SendEmail(chapter, [to], type, parameters);

    public async Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, EmailType type,
        IDictionary<string, string> parameters)
    {
        return await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            Parameters = parameters,
            To = to.ToArray(),
            Type = type
        });
    }

    public async Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, string subject, string body)
    {
        return await SendEmail(chapter, to, subject, body, new Dictionary<string, string>());
    }

    public async Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, string subject, string body,
        IDictionary<string, string> parameters)
    {
        return await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Parameters = parameters,
            Subject = subject,
            To = to.ToArray()
        });
    }

    public async Task<ServiceResult> SendMemberEmail(
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body)
    {
        return await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = [to]
        });
    }

    public async Task<ServiceResult> SendMemberEmail(
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body,
        IDictionary<string, string> parameters)
    {
        return await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = [to],
            Parameters = parameters
        });
    }

    private static IEnumerable<EmailAddressee> GetAddressees(IEnumerable<ChapterAdminMember> adminMembers)
    {
        foreach (var adminMember in adminMembers)
        {
            yield return adminMember.ToEmailAddressee();
        }
    }
}
