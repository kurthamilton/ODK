using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Settings;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Emails.Extensions;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Mails;

namespace ODK.Services.Emails;

public class MailProvider : IMailProvider
{    
    private readonly ILoggingService _loggingService;
    private readonly MailProviderSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MailProvider(IUnitOfWork unitOfWork, ILoggingService loggingService, 
        MailProviderSettings settings)
    {
        _loggingService = loggingService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> SendEmail(SendEmailOptions options)
    {
        var (emails, chapterEmails, providers, siteSettings, chapterSettings, summary) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetAll(),
            x => options.Chapter != null
                ? x.ChapterEmailRepository.GetByChapterId(options.Chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterEmail>(),
            x => x.EmailProviderRepository.GetAll(),
            x => x.SiteEmailSettingsRepository.Get(),
            x => options.Chapter != null
                ? x.ChapterEmailSettingsRepository.GetByChapterId(options.Chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<ChapterEmailSettings>(),
            x => x.EmailProviderRepository.GetEmailsSentToday());

        var layoutEmail = chapterEmails.FirstOrDefault(x => x.Type == EmailType.Layout)?.ToEmail() 
            ?? emails.First(x => x.Type == EmailType.Layout);

        var bodyEmail = options.Type != EmailType.Layout ? 
            chapterEmails.FirstOrDefault(x => x.Type == options.Type)?.ToEmail() ?? emails.First(x => x.Type == options.Type)
            : null;

        var parameters = options.Parameters ?? new Dictionary<string, string>();
        if (options.Chapter != null && !parameters.ContainsKey("chapter.name"))
        {
            parameters.Add("chapter.name", options.Chapter.Name);
        }

        var subject = StringUtils.Interpolate(!string.IsNullOrEmpty(options.Subject)
            ? options.Subject
            : bodyEmail?.Subject ?? "", parameters);

        var title = StringUtils.Interpolate(!string.IsNullOrEmpty(chapterSettings?.Title)
            ? chapterSettings.Title
            : siteSettings.Title, parameters);
        parameters.Add("title", title);        

        var body = StringUtils.Interpolate(!string.IsNullOrEmpty(options.Body)
            ? options.Body
            : bodyEmail?.HtmlContent ?? "", parameters);
        parameters.Add("body", body);

        var layoutBody = layoutEmail.HtmlContent;
        body = StringUtils.Interpolate(layoutBody, parameters);

        var (provider, _) = GetProvider(providers, summary);

        var message = CreateMessage(
            provider, 
            siteSettings, 
            chapterSettings, 
            options.FromAdminMember, 
            subject, 
            body);
        
        if (options.To.Count == 1)
        {
            var to = options.To.First().ToMailboxAddress();
            AddEmailRecipient(message, to);
        }
        else
        {
            var to = chapterSettings != null
                ? new MailboxAddress(chapterSettings.FromName, chapterSettings.FromEmailAddress)
                : new MailboxAddress(siteSettings.FromName, siteSettings.FromEmailAddress);

            var bcc = options.To;
            AddBulkEmailBccRecipients(message, to, bcc);
        }        

        return await SendEmail(provider, message);
    }

    private static void AddBulkEmailBccRecipients(
        MimeMessage message, 
        InternetAddress to, 
        IEnumerable<EmailAddressee> recipients)
    {
        AddEmailRecipient(message, to);

        foreach (EmailAddressee recipient in recipients)
        {
            message.Bcc.Add(recipient.ToMailboxAddress());
        }
    }

    private static void AddEmailRecipient(MimeMessage message, InternetAddress to)
    {
        message.To.Add(to);
    }

    private void AddEmailFrom(
        MimeMessage message, 
        EmailProvider provider, 
        SiteEmailSettings siteSettings,
        ChapterEmailSettings? chapterSettings,
        ChapterAdminMember? fromAdminMember)
    {
        if (fromAdminMember != null)
        {
            var emailAddress = !string.IsNullOrEmpty(fromAdminMember.AdminEmailAddress)
                ? fromAdminMember.AdminEmailAddress
                : fromAdminMember.Member.EmailAddress;

            message.From.Add(new MailboxAddress($"{fromAdminMember.Member.FullName}", emailAddress));
        }
        else
        {
            var name = !string.IsNullOrEmpty(chapterSettings?.FromName) 
                ? chapterSettings.FromName 
                : siteSettings.FromName;
            var address = !string.IsNullOrEmpty(chapterSettings?.FromEmailAddress) 
                ? chapterSettings.FromEmailAddress 
                : siteSettings.FromEmailAddress;
            message.From.Add(new MailboxAddress(name, address));
        }
    }

    private MimeMessage CreateMessage(
        EmailProvider provider, 
        SiteEmailSettings siteSettings,
        ChapterEmailSettings? chapterSettings,
        ChapterAdminMember? fromAdminMember, 
        string subject, 
        string body)
    {
        var message = new MimeMessage
        {
            Body = new TextPart(TextFormat.Html)
            {
                Text = body
            },
            Subject = subject
        };

        AddEmailFrom(message, provider, siteSettings, chapterSettings, fromAdminMember);

        return message;
    }

    private string GetLayoutBody(Email email, Chapter? chapter, string body)
    {
        var layout = email.Interpolate(new Dictionary<string, string>
        {
            { "chapter.name", chapter?.Name ?? "" },
            { "body", body }
        });

        return layout.HtmlContent;
    }

    private (EmailProvider Provider, int Remaining) GetProvider(
        IReadOnlyCollection<EmailProvider> providers, 
        IReadOnlyCollection<EmailProviderSummaryDto> summary)
    {
        var summaryDictionary = summary
            .ToDictionary(x => x.EmailProviderId, x => x.Sent);

        foreach (var provider in providers)
        {
            summaryDictionary.TryGetValue(provider.Id, out var sentToday);            
            if (sentToday < provider.DailyLimit)
            {
                return (provider, provider.DailyLimit - sentToday);
            }
        }

        throw new OdkServiceException("No more emails can be sent today");
    }

    private async Task<ServiceResult> SendEmail(EmailProvider provider, MimeMessage message)
    {
        if (message.To.Count == 0)
        {
            await _loggingService.LogDebug("Not sending email, no recipients set");
            return ServiceResult.Failure("No recipients set");
        }

        await _loggingService.LogDebug($"Sending email to {string.Join(", ", message.To)}");

        if (!string.IsNullOrEmpty(_settings.DebugEmailAddress))
        {
            await _loggingService.LogDebug($"Sending to debug email address {_settings.DebugEmailAddress}");
            message.To.Clear();
            message.Cc.Clear();
            message.Bcc.Clear();
            message.To.Add(new MailboxAddress("", _settings.DebugEmailAddress));
        }

        try
        {
            using var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };
            await client.ConnectAsync(provider.SmtpServer, provider.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable);
            await client.AuthenticateAsync(provider.SmtpLogin, provider.SmtpPassword);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            foreach (var to in message.To.Union(message.Cc).Union(message.Bcc))
            {
                _unitOfWork.EmailRepository.AddSentEmail(new SentEmail
                {                    
                    SentUtc = DateTime.UtcNow,
                    EmailProviderId = provider.Id,
                    Subject = message.Subject,
                    To = to.ToString()
                });                                
            }

            await _unitOfWork.SaveChangesAsync();

            await _loggingService.LogDebug($"Email sent to {string.Join(", ", message.To)}");
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            await _loggingService.LogError(ex, new Dictionary<string, string>
            {
                { "MAIL.TO", string.Join(", ", message.To) },
                { "MAIL.HTMLBODY", message.HtmlBody },
                { "MAIL.SUBJECT", message.Subject }
            });
            return ServiceResult.Failure($"Error sending email: {ex.Message}");
        }
    }
}
