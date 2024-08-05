using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Settings;
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

    public async Task SendBulkEmail(
        Chapter chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body, 
        bool bcc = true)
    {
        await SendBulkEmail(chapter, to, subject, body, null, bcc);
    }

    public async Task SendBulkEmail(
        Chapter chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body, 
        ChapterAdminMember? fromAdminMember,
        bool bcc = true)
    {
        var (email, chapterEmail, providers, siteSettings, chapterSettings, summary) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetByType(EmailType.Layout),
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, EmailType.Layout),
            x => x.EmailProviderRepository.GetAll(),
            x => x.SiteSettingsRepository.Get(),
            x => x.ChapterEmailSettingsRepository.GetByChapterId(chapter.Id),
            x => x.EmailProviderRepository.GetEmailsSentToday());
        body = GetLayoutBody(chapterEmail?.ToEmail() ?? email, chapter, body);

        int i = 0;
        var toList = to.ToList();
        while (i < toList.Count)
        {
            var (provider, remaining) = GetProvider(providers, summary);

            int batchSize = provider.BatchSize ?? (toList.Count - i);
            if (batchSize > remaining)
            {
                batchSize = remaining;
            }

            var batch = toList.Skip(i).Take(batchSize);
            var message = CreateMessage(provider, siteSettings, chapterSettings, fromAdminMember, subject, body);
            if (bcc)
            {
                AddBulkEmailBccRecipients(message, message.From.First(), batch);
            }
            else
            {
                AddBulkEmailRecipients(message, batch);
            }

            await SendEmail(provider, message);
            i += batchSize;
        }
    }

    public Task<ServiceResult> SendEmail(Chapter? chapter, EmailAddressee to, string subject, string body)    
        => SendEmail(chapter, to, subject, body, null);

    public async Task<ServiceResult> SendEmail(
        Chapter? chapter, 
        EmailAddressee to, 
        string subject, 
        string body, 
        ChapterAdminMember? fromAdminMember)
    {
        var (email, chapterEmail, providers, siteSettings, chapterSettings, summary) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetByType(EmailType.Layout),
            x => chapter != null 
                ? x.ChapterEmailRepository.GetByChapterId(chapter.Id, EmailType.Layout) 
                : new DefaultDeferredQuerySingleOrDefault<ChapterEmail>(),
            x => x.EmailProviderRepository.GetAll(),
            x => x.SiteSettingsRepository.Get(),
            x => chapter != null 
                ? x.ChapterEmailSettingsRepository.GetByChapterId(chapter.Id)
                : new DefaultDeferredQuerySingleOrDefault<ChapterEmailSettings>(),
            x => x.EmailProviderRepository.GetEmailsSentToday());
        
        body = GetLayoutBody(chapterEmail?.ToEmail() ?? email, chapter, body);
        
        var (provider, _) = GetProvider(providers, summary);

        var message = CreateMessage(provider, siteSettings, chapterSettings, fromAdminMember, subject, body);
        AddEmailRecipient(message, to.ToMailboxAddress());

        return await SendEmail(provider, message);
    }

    private static void AddBulkEmailRecipients(MimeMessage message, IEnumerable<EmailAddressee> recipients)
    {
        foreach (EmailAddressee recipient in recipients)
        {
            message.To.Add(recipient.ToMailboxAddress());
        }
    }

    private static void AddBulkEmailBccRecipients(MimeMessage message, InternetAddress to, IEnumerable<EmailAddressee> recipients)
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
        SiteSettings siteSettings,
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
            var name = !string.IsNullOrEmpty(chapterSettings?.FromName) ? chapterSettings.FromName : siteSettings.DefaultFromEmailName;
            var address = !string.IsNullOrEmpty(chapterSettings?.FromEmailAddress) ? chapterSettings.FromEmailAddress : siteSettings.DefaultFromEmailAddress;
            message.From.Add(new MailboxAddress(name, address));
        }
    }

    private MimeMessage CreateMessage(
        EmailProvider provider, 
        SiteSettings siteSettings,
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
            using SmtpClient client = new SmtpClient
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
