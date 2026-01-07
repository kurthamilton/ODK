using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Extensions;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Smtp;

public class SmtpEmailClient : IEmailClient
{
    private readonly ILoggingService _loggingService;
    private readonly SmtpEmailClientSettings _settings;

    public SmtpEmailClient(
        ILoggingService loggingService,
        SmtpEmailClientSettings settings)
    {
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<SendEmailResult> SendEmail(EmailProvider provider, EmailClientEmail email)
    {
        var message = email.ToMimeMessage();

        if (message.To.Count == 0)
        {
            await _loggingService.Info("Not sending email, no recipients set");
            return new SendEmailResult(false, "No recipients set")
            {
                ExternalId = null
            };
        }

        await _loggingService.Info($"Sending email to {string.Join(", ", message.To)}");

        if (!string.IsNullOrEmpty(_settings.DebugEmailAddress))
        {
            await _loggingService.Info($"Sending to debug email address {_settings.DebugEmailAddress}");
            message.To.Clear();
            message.Cc.Clear();
            message.Bcc.Clear();
            message.To.Add(new MailboxAddress(string.Empty, _settings.DebugEmailAddress));
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

            await _loggingService.Info($"Email sent to {string.Join(", ", message.To)}");
            return new SendEmailResult(true)
            {
                ExternalId = smtpResponse.ExternalId
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error(ex, new Dictionary<string, string>
            {
                { "MAIL.TO", string.Join(", ", message.To) },
                { "MAIL.HTMLBODY", message.HtmlBody },
                { "MAIL.SUBJECT", message.Subject }
            });

            return new SendEmailResult(false, $"Error sending email: {ex.Message}")
            {
                ExternalId = null
            };
        }
    }
}
