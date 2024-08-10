using System.Web;
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

    public EmailService(IUnitOfWork unitOfWork, IMailProvider mailProvider)
    {
        _mailProvider = mailProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task SendBulkEmail(
        ChapterAdminMember? fromAdminMember, 
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters)
    {
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Parameters = parameters,
            Subject = "",
            To = to.Select(x => x.GetEmailAddressee()).ToArray(),
            Type = type
        };

        await _mailProvider.SendEmail(options);
    }

    public async Task SendBulkEmail(
        ChapterAdminMember fromAdminMember,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        var options = new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Subject = subject,
            To = to.Select(x => x.GetEmailAddressee()).ToArray(),
        };

        await _mailProvider.SendEmail(options);
    }

    public async Task SendContactEmail(string fromAddress, string message)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"from", fromAddress},
            {"message", HttpUtility.HtmlEncode(message)}
        };

        var settings = await _unitOfWork.SiteEmailSettingsRepository
            .Get()
            .RunAsync();

        var to = new EmailAddressee(settings.ContactEmailAddress, "");

        var options = new SendEmailOptions
        {
            Body = "",
            Parameters = parameters,
            Subject = "",
            To = [to],
            Type = EmailType.ContactRequest
        };

        await _mailProvider.SendEmail(options);
    }

    public async Task SendContactEmail(Chapter chapter, string from, string message)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"from", from},
            {"message", HttpUtility.HtmlEncode(message)}
        };

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(chapter.Id)
            .RunAsync();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveContactEmails));

        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Parameters = parameters,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.ContactRequest
        };

        await _mailProvider.SendEmail(options);
    }

    public async Task SendEventCommentEmail(Chapter chapter, Member? replyToMember, EventComment comment,
        IDictionary<string, string> parameters)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(chapter.Id)
            .RunAsync();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveEventCommentEmails));
        if (replyToMember != null)
        {
            to = to.Append(new EmailAddressee(replyToMember.EmailAddress, replyToMember.FullName));
        }

        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Parameters = parameters,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.EventComment
        };

        await _mailProvider.SendEmail(options);
    }
    
    public async Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, EmailType type, 
        IDictionary<string, string> parameters)
    {        
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            Parameters = parameters,
            To = [to],
            Type = type
        };

        return await _mailProvider.SendEmail(options);
    }

    public async Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, string subject, string body)
    {
        var options = new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = [to]
        };

        return await _mailProvider.SendEmail(options);
    }

    public async Task<ServiceResult> SendMemberEmail(Chapter chapter, ChapterAdminMember fromAdminMember, Member to, string subject, string body)
    {
        var options = new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Subject = subject,
            To = [to.GetEmailAddressee()]
        };
        return await _mailProvider.SendEmail(options);
    }

    public async Task SendNewMemberAdminEmail(Chapter chapter, Member member, 
        IDictionary<string, string> parameters)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
                .GetByChapterId(chapter.Id)
                .RunAsync();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.SendNewMemberEmails));
        
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.NewMemberAdmin
        };
    }

    private static IEnumerable<EmailAddressee> GetAddressees(IEnumerable<ChapterAdminMember> adminMembers)
    {
        foreach (var adminMember in adminMembers)
        {
            var member = adminMember.Member;
            var address = !string.IsNullOrEmpty(adminMember.AdminEmailAddress)
                ? adminMember.AdminEmailAddress
                : member.EmailAddress;
            yield return new EmailAddressee(address, member.FullName);
        }
    }

    private static Email GetEmail(Email email, IDictionary<string, string> parameters)
    {
        return email.Interpolate(parameters);
    }
}
