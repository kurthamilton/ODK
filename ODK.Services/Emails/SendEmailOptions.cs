using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Services.Emails;

public class SendEmailOptions
{
    public required string Body { get; init; }

    public Chapter? Chapter { get; init; }

    public IEmailParameters? Parameters { get; init; }

    public required string Subject { get; init; }

    /// <summary>
    /// Who the email is written for, for a send that carries its own subject and body and so has no email
    /// row to declare it. Left unset where <see cref="Type"/> names an email, whose own
    /// <see cref="Email.RecipientType"/> answers it instead.
    /// </summary>
    public EmailRecipientType RecipientType { get; init; }

    public required IReadOnlyCollection<EmailAddressee> To { get; init; }

    public EmailType Type { get; init; } = EmailType.Layout;
}
