using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Extensions;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Smtp;

public class SmtpEmailClient : IEmailClient
{
    private readonly ILoggingService _loggingService;
    private readonly SmtpEmailClientSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    
    public SmtpEmailClient(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        SmtpEmailClientSettings settings,
        IPlatformProvider platformProvider,
        IUrlProvider urlProvider)
    {
        _loggingService = loggingService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> SendEmail(EmailProvider provider, EmailClientEmail email)
    {
        var message = email.ToMimeMessage();

        if (message.To.Count == 0)
        {
            await _loggingService.Info("Not sending email, no recipients set");
            return ServiceResult.Failure("No recipients set");
        }

        await _loggingService.Info($"Sending email to {string.Join(", ", message.To)}");

        if (!string.IsNullOrEmpty(_settings.DebugEmailAddress))
        {
            await _loggingService.Info($"Sending to debug email address {_settings.DebugEmailAddress}");
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

            var response = await client.SendAsync(message);
            var smtpResponse = SmtpResponse.Parse(response);

            await _loggingService.Info($"SMTP response: {response}");

            if (!smtpResponse.Success)
            {
                await _loggingService.Error("SMTP email client returned a possible error response");
            }

            await client.DisconnectAsync(true);

            var chapterProvider = provider as ChapterEmailProvider;

            foreach (var to in message.To.Union(message.Cc).Union(message.Bcc))
            {
                _unitOfWork.EmailRepository.AddSentEmail(new SentEmail
                {
                    ExternalId = smtpResponse.ExternalId,
                    SentUtc = DateTime.UtcNow,
                    ChapterEmailProviderId = chapterProvider?.Id,
                    EmailProviderId = chapterProvider == null ? provider.Id : null,
                    Subject = message.Subject,
                    To = to.ToString()
                });
            }

            await _unitOfWork.SaveChangesAsync();

            await _loggingService.Info($"Email sent to {string.Join(", ", message.To)}");
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            await _loggingService.Error(ex, new Dictionary<string, string>
            {
                { "MAIL.TO", string.Join(", ", message.To) },
                { "MAIL.HTMLBODY", message.HtmlBody },
                { "MAIL.SUBJECT", message.Subject }
            });
            return ServiceResult.Failure($"Error sending email: {ex.Message}");
        }
    }
}
