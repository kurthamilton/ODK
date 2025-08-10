using MimeKit;
using MimeKit.Text;
using ODK.Core.Emails;
using ODK.Services.Emails;

namespace ODK.Services.Integrations.Emails.Extensions;

internal static class EmailClientEmailExtensions
{
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
