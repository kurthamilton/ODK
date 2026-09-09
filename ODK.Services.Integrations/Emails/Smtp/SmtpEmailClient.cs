using MailKit.Net.Smtp;
using MailKit.Security;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Extensions;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Emails.Smtp;

/// <summary>
/// An <see cref="IEmailClient"/> that delivers over SMTP to a local mail sink, so local development sees
/// the messages the app would send - every recipient, and the body as rendered - without reaching the real
/// email provider. Selected when <c>Emails.Client</c> is <c>Smtp</c>.
/// </summary>
/// <remarks>
/// The sink runs on loopback and accepts anything, so the connection is unencrypted and unauthenticated. A
/// send returns no external id: a delivery webhook is correlated by the provider's message id, so nothing
/// tracks the status of a message sent this way.
/// </remarks>
public class SmtpEmailClient : IEmailClient
{
    private readonly ILoggingService _loggingService;
    private readonly SmtpEmailClientSettings _settings;

    public SmtpEmailClient(
        SmtpEmailClientSettings settings,
        ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<SendEmailResult> SendEmail(EmailClientEmail email)
    {
        if (email.To.Count == 0)
        {
            await _loggingService.Info("Not sending email, no recipients set");
            return new SendEmailResult(false, "No recipients set")
            {
                ExternalId = null
            };
        }

        if (email.ScheduledUtc != null)
        {
            await _loggingService.Info(
                "[SmtpEmailClient] Sending now - SMTP carries no schedule " +
                $"(requested {email.ScheduledUtc:O})");
        }

        await _loggingService.Info($"Sending email to {email.To.Count} recipient(s)");

        var message = email.ToMimeMessage();

        try
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.None);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return new SendEmailResult(true)
            {
                ExternalId = null
            };
        }
        catch (Exception ex)
        {
            // Don't log recipient addresses or the email body (PII); a count + subject is enough to
            // diagnose a send failure.
            await _loggingService.Error(ex, new Dictionary<string, string?>
            {
                { "MAIL.TO.COUNT", email.To.Count.ToString() },
                { "MAIL.SUBJECT", email.Subject }
            });

            return new SendEmailResult(false, $"Error sending email: {ex.Message}")
            {
                ExternalId = null
            };
        }
    }
}
