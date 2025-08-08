using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Exceptions;

namespace ODK.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IEmailClientFactory _emailClientFactory;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public EmailService(
        IUnitOfWork unitOfWork,
        IEmailClientFactory emailClientFactory,
        IUrlProvider urlProvider,
        IPlatformProvider platformProvider)
    {
        _emailClientFactory = emailClientFactory;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        EmailType type,
        IDictionary<string, string> parameters)
    {
        await SendEmail(new SendEmailOptions
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
        await SendEmail(new SendEmailOptions
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

        await SendEmail(new SendEmailOptions
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
        return await SendEmail(new SendEmailOptions
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
        return await SendEmail(new SendEmailOptions
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
        return await SendEmail(new SendEmailOptions
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
        return await SendEmail(new SendEmailOptions
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

    private static EmailProvider GetProvider(
        IReadOnlyCollection<EmailProvider> siteProviders,
        IReadOnlyCollection<ChapterEmailProvider> chapterProviders,
        IReadOnlyCollection<EmailProviderSummaryDto> siteSummary,
        IReadOnlyCollection<EmailProviderSummaryDto> chapterSummary)
    {
        var chapterSummaryDictionary = chapterSummary
            .ToDictionary(x => x.EmailProviderId, x => x.Sent);
        var siteSummaryDictionary = siteSummary
            .ToDictionary(x => x.EmailProviderId, x => x.Sent);

        foreach (var provider in chapterProviders.OrderBy(x => x.Order))
        {
            chapterSummaryDictionary.TryGetValue(provider.Id, out var sentToday);
            if (sentToday < provider.DailyLimit)
            {
                return provider;
            }
        }

        foreach (var provider in siteProviders.OrderBy(x => x.Order))
        {
            siteSummaryDictionary.TryGetValue(provider.Id, out var sentToday);
            if (sentToday < provider.DailyLimit)
            {
                return provider;
            }
        }

        throw new OdkServiceException("No more emails can be sent today");
    }

    private async Task<ServiceResult> SendEmail(SendEmailOptions options)
    {
        var platform = _platformProvider.GetPlatform();

        var (emails, chapterEmails, chapterProviders, siteProviders, siteSettings, siteSummary, chapterSummary) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetAll(),
            x => options.Chapter != null
                ? x.ChapterEmailRepository.GetByChapterId(options.Chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterEmail>(),
            x => options.Chapter != null
                ? x.ChapterEmailProviderRepository.GetByChapterId(options.Chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterEmailProvider>(),
            x => x.EmailProviderRepository.GetAll(),
            x => x.SiteEmailSettingsRepository.Get(platform),
            x => x.EmailProviderRepository.GetEmailsSentToday(),
            x => options.Chapter != null
                ? x.ChapterEmailProviderRepository.GetEmailsSentToday(options.Chapter.Id)
                : new DefaultDeferredQueryMultiple<EmailProviderSummaryDto>());

        var layoutEmail = chapterEmails.FirstOrDefault(x => x.Type == EmailType.Layout)?.ToEmail()
            ?? emails.First(x => x.Type == EmailType.Layout);

        var bodyEmail = options.Type != EmailType.Layout ?
            chapterEmails.FirstOrDefault(x => x.Type == options.Type)?.ToEmail() ?? emails.First(x => x.Type == options.Type)
            : null;

        var parameters = options.Parameters ?? new Dictionary<string, string>();
        if (!parameters.ContainsKey("chapter.name"))
        {
            parameters["chapter.name"] = options.Chapter?.Name ?? "";
        }

        if (options.Chapter != null)
        {
            parameters["chapter.baseurl"] = _urlProvider.GroupUrl(options.Chapter);
        }

        if (!parameters.ContainsKey("platform.baseurl"))
        {
            parameters["platform.baseurl"] = _urlProvider.BaseUrl();
        }

        var title = siteSettings.Title.Interpolate(parameters.AsReadOnly(), HttpUtility.HtmlEncode);
        parameters["title"] = title;

        var subject = !string.IsNullOrEmpty(options.Subject)
            ? options.Subject
            : bodyEmail?.Subject ?? "";

        var body = !string.IsNullOrEmpty(options.Body)
            ? options.Body
            : bodyEmail?.HtmlContent ?? "";
        body = body.Interpolate(parameters.AsReadOnly(), HttpUtility.HtmlEncode);

        foreach (var htmlParameter in parameters.Where(x => x.Key.StartsWith("html:")))
        {
            var parameterName = htmlParameter.Key.Substring("html:".Length);
            body = body.Interpolate(new Dictionary<string, string>
            {
                { parameterName, parameters[htmlParameter.Key] }
            });
        }

        parameters["body"] = body;

        var layoutBody = layoutEmail.HtmlContent;
        body = layoutBody.Interpolate(parameters.AsReadOnly());

        var emailClientEmail = new EmailClientEmail
        {
            Body = body,
            From = new EmailAddressee(
                siteSettings.FromEmailAddress, 
                siteSettings.FromName.Interpolate(parameters.AsReadOnly())),
            Subject = subject.Interpolate(parameters.AsReadOnly(), HttpUtility.HtmlEncode),
            To = options.To
        };

        var provider = GetProvider(siteProviders, chapterProviders, siteSummary, chapterSummary);
        var emailClient = _emailClientFactory.Create(provider.Type);
        return await emailClient.SendEmail(provider, emailClientEmail);
    }
}
