using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
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
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Parameters = parameters,
            Subject = "",
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
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
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
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

        var platform = _platformProvider.GetPlatform();

        var settings = await _unitOfWork.SiteEmailSettingsRepository
            .Get(platform)
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
    
    public Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, EmailType type, 
        IDictionary<string, string> parameters) 
        => SendEmail(chapter, [to], type, parameters);

    public async Task<ServiceResult> SendEmail(Chapter? chapter, IEnumerable<EmailAddressee> to, EmailType type,
        IDictionary<string, string> parameters)
    {
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            Parameters = parameters,
            To = to.ToArray(),
            Type = type
        };

        return await _mailProvider.SendEmail(options);
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
        var options = new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            FromAdminMember = fromAdminMember,
            Subject = subject,
            To = [to],
            Parameters = parameters
        };
        return await _mailProvider.SendEmail(options);
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
                .RunAsync();

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveNewMemberEmails));
        
        var options = new SendEmailOptions
        {
            Body = "",
            Chapter = chapter,
            Subject = "",
            To = to.ToArray(),
            Type = EmailType.NewMemberAdmin,
            Parameters = parameters
        };
        await _mailProvider.SendEmail(options);
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
