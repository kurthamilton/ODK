using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;

namespace ODK.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IMailProvider _mailProvider;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public EmailService(
        IUnitOfWork unitOfWork, 
        IMailProvider mailProvider,
        IPlatformProvider platformProvider,
        IUrlProvider urlProvider)
    {
        _mailProvider = mailProvider;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task SendBulkEmail(
        ChapterAdminMember? fromAdminMember, 
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters)
    {
        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Parameters = parameters,
            Subject = "",
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
            Type = type
        });
    }

    public async Task SendBulkEmail(
        ChapterAdminMember fromAdminMember,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Subject = subject,
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
        });
    }

    public async Task SendContactEmail(SiteContactMessage message)
    {
        var platform = _platformProvider.GetPlatform();

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"message.from", message.FromAddress},
            {"message.text", HttpUtility.HtmlEncode(message.Message)},
            {"url", _urlProvider.MessageAdminUrl(platform, message.Id)}
        };        

        var settings = await _unitOfWork.SiteEmailSettingsRepository
            .Get(platform)
            .Run();

        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Parameters = parameters,
            Subject = "",
            To = [new EmailAddressee(settings.ContactEmailAddress, "")],
            Type = EmailType.ContactRequest
        });
    }

    public async Task SendContactEmail(Chapter chapter, ChapterContactMessage message)
    {
        var platform = _platformProvider.GetPlatform();

        var url = _urlProvider.MessageAdminUrl(platform, chapter, message.Id);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"message.from", message.FromAddress},
            {"message.text", HttpUtility.HtmlEncode(message.Message)},
            {"url", url}
        };

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(chapter.Id)
            .Run();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveContactEmails));

        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Parameters = parameters,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.ContactRequest
        });
    }

    public async Task SendEventCommentEmail(Chapter chapter, Member? replyToMember, EventComment comment,
        IDictionary<string, string> parameters)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(chapter.Id)
            .Run();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveEventCommentEmails));
        if (replyToMember != null)
        {
            to = to.Append(new EmailAddressee(replyToMember.EmailAddress, replyToMember.FullName));
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
        ChapterAdminMember? fromAdminMember,
        EmailAddressee to, 
        string subject, 
        string body)
    {
        return await SendMemberEmail(chapter, fromAdminMember, to, subject, body, new Dictionary<string, string>());
    }

    public async Task<ServiceResult> SendMemberEmail(
        Chapter? chapter, 
        ChapterAdminMember? fromAdminMember,
        EmailAddressee to, 
        string subject, 
        string body,
        IDictionary<string, string> parameters)
    {
        return await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Subject = subject,
            To = [to],
            Parameters = parameters
        });
    }

    public async Task SendNewChapterMemberEmail(Chapter chapter, Member member)
    {
        var platform = _platformProvider.GetPlatform();

        var eventsUrl = _urlProvider.EventsUrl(platform, chapter);

        await SendEmail(chapter, member.ToEmailAddressee(), EmailType.NewMember, new Dictionary<string, string>
        {
            { "eventsUrl", eventsUrl },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) }
        });
    }

    public async Task SendNewMemberAdminEmail(Chapter chapter, Member member, 
        IDictionary<string, string> parameters)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
                .GetByChapterId(chapter.Id)
                .Run();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveNewMemberEmails))
            .ToArray();

        if (to.Length == 0)
        {
            return;
        }

        await _mailProvider.SendEmail(new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            To = to,
            Type = EmailType.NewMemberAdmin,
            Parameters = parameters
        });
    }

    public async Task SendNewMemberAdminEmail(
        Chapter chapter, 
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var parameters = new Dictionary<string, string>
        {
            { "member.emailAddress", HttpUtility.HtmlEncode(member.EmailAddress) },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) },
            { "member.lastName", HttpUtility.HtmlEncode(member.LastName) }
        };

        foreach (var chapterProperty in chapterProperties)
        {
            string? value = memberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id)?.Value;
            parameters.Add($"member.properties.{chapterProperty.Name}", HttpUtility.HtmlEncode(value ?? ""));
        }

        await SendNewMemberAdminEmail(chapter, member, parameters);
    }

    private static IEnumerable<EmailAddressee> GetAddressees(IEnumerable<ChapterAdminMember> adminMembers)
    {
        foreach (var adminMember in adminMembers)
        {
            yield return adminMember.ToEmailAddressee();
        }
    }

    private static Email GetEmail(Email email, IDictionary<string, string> parameters)
    {
        return email.Interpolate(parameters);
    }
}
