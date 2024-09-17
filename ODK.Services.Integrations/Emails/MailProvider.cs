using System.Web;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Integrations.Emails.Extensions;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails;

public class MailProvider : IMailProvider
{
    private readonly ILoggingService _loggingService;
    private readonly IPlatformProvider _platformProvider;
    private readonly MailProviderSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MailProvider(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        MailProviderSettings settings,
        IPlatformProvider platformProvider)
    {
        _loggingService = loggingService;
        _platformProvider = platformProvider;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> SendEmail(SendEmailOptions options)
    {
        var platform = _platformProvider.GetPlatform();

        var (emails, chapterEmails, providers, siteSettings, summary) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetAll(),
            x => options.Chapter != null
                ? x.ChapterEmailRepository.GetByChapterId(options.Chapter.Id)
                : new DefaultDeferredQueryMultiple<ChapterEmail>(),
            x => x.EmailProviderRepository.GetAll(),
            x => x.SiteEmailSettingsRepository.Get(platform),
            x => x.EmailProviderRepository.GetEmailsSentToday());

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

        var platformBaseUrl = _platformProvider.GetBaseUrl();

        var chapterUrl = options.Chapter != null
            ? platform == PlatformType.DrunkenKnitwits
                ? $"{platformBaseUrl}/{options.Chapter.Name}"
                : $"{platformBaseUrl}/groups/{options.Chapter.Slug}"
            : null;
        if (chapterUrl != null)
        {
            parameters["chapter.baseurl"] = chapterUrl;
        }

        if (!parameters.ContainsKey("platform.baseurl"))
        {
            if (platform == PlatformType.DrunkenKnitwits && options.Chapter != null)
            {
                platformBaseUrl += $"/{options.Chapter.Name}";
            }

            parameters["platform.baseurl"] = platformBaseUrl;
        }

        var title = siteSettings.Title.Interpolate(parameters,
            HttpUtility.HtmlEncode);
        parameters["title"] = title;

        var subject = (!string.IsNullOrEmpty(options.Subject)
            ? options.Subject
            : bodyEmail?.Subject ?? "").Interpolate(parameters,
            HttpUtility.HtmlEncode);

        var body = (!string.IsNullOrEmpty(options.Body)
            ? options.Body
            : bodyEmail?.HtmlContent ?? "").Interpolate(parameters,
            HttpUtility.HtmlEncode);
        parameters["body"] = body;

        var layoutBody = layoutEmail.HtmlContent;
        body = layoutBody.Interpolate(parameters);

        var (provider, _) = GetProvider(providers, summary);

        var message = CreateMessage(
            provider,
            siteSettings,
            subject,
            body, parameters);

        if (options.To.Count == 1)
        {
            var to = options.To.First().ToMailboxAddress();
            AddEmailRecipient(message, to);
        }
        else
        {
            var name = siteSettings.FromName.Interpolate(parameters);
            var to = new MailboxAddress(name, siteSettings.FromEmailAddress);

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
        IDictionary<string, string> parameters)
    {
        var name = siteSettings.FromName.Interpolate(parameters);
        var address = siteSettings.FromEmailAddress;
        message.From.Add(new MailboxAddress(name, address));
    }

    private MimeMessage CreateMessage(
        EmailProvider provider,
        SiteEmailSettings siteSettings,
        string subject,
        string body,
        IDictionary<string, string> parameters)
    {
        var message = new MimeMessage
        {
            Body = new TextPart(TextFormat.Html)
            {
                Text = body
            },
            Subject = subject
        };

        AddEmailFrom(message, provider, siteSettings, parameters);

        return message;
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
