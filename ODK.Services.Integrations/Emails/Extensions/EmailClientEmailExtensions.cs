using MimeKit;
using MimeKit.Text;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Services.Integrations.Emails.Brevo.Models;

namespace ODK.Services.Integrations.Emails.Extensions;

internal static class EmailClientEmailExtensions
{
    internal static BrevoTransactionalEmailRequest ToBrevoRequest(
        this EmailClientEmail email,
        string? debugEmailAddress)
    {
        var sender = new BrevoEmailAddressee(email.From);

        var to = new List<BrevoEmailAddressee>();
        var bcc = new List<BrevoEmailAddressee>();

        if (!string.IsNullOrEmpty(debugEmailAddress))
        {
            to.Add(new BrevoEmailAddressee(debugEmailAddress));
        }
        else if (email.To.Count == 1)
        {
            var toAddressee = email.To.First();
            to.Add(new BrevoEmailAddressee(toAddressee));
        }
        else
        {
            to.Add(sender);
            bcc.AddRange(email.To.Select(x => new BrevoEmailAddressee(x)));
        }

        return new BrevoTransactionalEmailRequest
        {
            Bcc = bcc.Count > 0 ? bcc : null,
            HtmlContent = email.Body,
            ScheduledAt = email.ScheduledUtc,
            Sender = sender,
            Subject = email.Subject,
            To = to
        };
    }

    internal static MimeMessage ToMimeMessage(this EmailClientEmail email)
    {
        var message = new MimeMessage
        {
            Body = new TextPart(TextFormat.Html)
            {
                Text = email.Body
            },
            Subject = email.Subject
        };

        var from = email.From.ToMailboxAddress();
        message.From.Add(from);

        if (email.To.Count == 1)
        {
            var to = email.To.First().ToMailboxAddress();
            message.To.Add(to);
        }
        else
        {
            var to = email.From.ToMailboxAddress();
            message.To.Add(to);

            foreach (var recipient in email.To)
            {
                message.Bcc.Add(recipient.ToMailboxAddress());
            }
        }

        return message;
    }
}