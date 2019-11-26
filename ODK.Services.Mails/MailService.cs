using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Chapters;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public class MailService : IMailService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberEmailRepository _memberEmailRepository;
        private readonly SmtpSettings _smtpSettings;

        public MailService(SmtpSettings smtpSettings, IChapterRepository chapterRepository, IMemberEmailRepository memberEmailRepository)
        {
            _chapterRepository = chapterRepository;
            _memberEmailRepository = memberEmailRepository;
            _smtpSettings = smtpSettings;
        }

        public async Task SendMail(Member member, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type);

            StringBuilder body = new StringBuilder(email.Body);
            StringBuilder subject = new StringBuilder(email.Subject);
            foreach (string key in parameters.Keys)
            {
                body.Replace($"{{{key}}}", parameters[key]);
                subject.Replace($"{{{key}}}", parameters[key]);
            }

            await SendMail(member, subject.ToString(), body.ToString());
        }

        public async Task SendMail(Member member, string subject, string body)
        {
            await SendMail(member.ChapterId, member.EmailAddress, subject, body);
        }

        private async Task SendMail(Guid chapterId, string to, string subject, string body)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapterId);

            MemberEmail memberEmail = new MemberEmail(Guid.Empty, chapterId, to, subject, DateTime.UtcNow, false);
            Guid memberEmailId = await _memberEmailRepository.AddMemberEmail(memberEmail);

            await Send(emailSettings.FromEmailAddress, to, subject, body);
            await _memberEmailRepository.ConfirmMemberEmailSent(memberEmailId);
        }

        private async Task Send(string from, string to, string subject, string body)
        {
            MimeMessage message = new MimeMessage
            {
                Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                },
                Subject = subject
            };

            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(to));

            using SmtpClient client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, 25, false);

            if (!string.IsNullOrWhiteSpace(_smtpSettings.Username) &&
                !string.IsNullOrWhiteSpace(_smtpSettings.Password))
            {
                client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
