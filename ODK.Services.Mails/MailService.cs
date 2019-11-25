using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace ODK.Services.Mails
{
    public class MailService : IMailService
    {
        private readonly SmtpSettings _smtpSettings;

        public MailService(SmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }

        public async Task SendMail(string from, IEnumerable<string> to, string subject, string body)
        {
            var message = new MimeMessage
            {
                Body = new TextPart("plain")
                {
                    Text = body
                },
                Subject = subject
            };

            message.From.Add(new MailboxAddress(from));
            foreach (string toAddress in to)
            {
                message.To.Add(new MailboxAddress(toAddress));
            }

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Host, 25, false);
                                
                client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
