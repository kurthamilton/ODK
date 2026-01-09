namespace ODK.Services.Integrations.Emails.Brevo.Models;

public class BrevoTransactionalEmailRequest
{
    public IReadOnlyCollection<BrevoEmailAddressee>? Bcc { get; init; }

    public IReadOnlyCollection<BrevoEmailAddressee>? Cc { get; init; }

    public required string HtmlContent { get; init; }

    public required DateTime? ScheduledAt { get; init; }

    public required BrevoEmailAddressee Sender { get; init; }

    public required string Subject { get; init; }

    public required IReadOnlyCollection<BrevoEmailAddressee> To { get; init; }
}
