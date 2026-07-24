using ODK.Services.Logging;

namespace ODK.Services.Emails;

/// <summary>
/// An <see cref="IEmailClient"/> that logs emails instead of sending them, so end-to-end tests (and
/// local development) don't hit the real email provider and consume quota. Selected when
/// <c>Emails.UseConsoleClient</c> is enabled. Output goes through the app logger, which writes to the
/// console when the app is run from a terminal.
/// </summary>
public class ConsoleEmailClient : IEmailClient
{
    private readonly ILoggingService _loggingService;

    public ConsoleEmailClient(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<SendEmailResult> SendEmail(EmailClientEmail email)
    {
        var recipients = string.Join(", ", email.To.Select(x => x.ToString()));

        await _loggingService.Info(
            $"[ConsoleEmailClient] Email NOT sent (console client enabled).{Environment.NewLine}" +
            $"  To: {recipients}{Environment.NewLine}" +
            $"  From: {email.From}{Environment.NewLine}" +
            $"  Scheduled: {email.ScheduledUtc?.ToString("O") ?? "immediate"}{Environment.NewLine}" +
            $"  Subject: {email.Subject}{Environment.NewLine}" +
            $"  Body:{Environment.NewLine}{email.Body}");

        return new SendEmailResult(true, "Logged by ConsoleEmailClient")
        {
            ExternalId = null
        };
    }
}
