using System;
using System.Collections.Generic;
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

        public async Task<MemberEmail> CreateMemberEmail(Member member, Email email, IDictionary<string, string> parameters)
        {
            email = email.Interpolate(parameters);

            MemberEmail memberEmail = new MemberEmail(Guid.Empty, member.ChapterId, member.EmailAddress, email.Subject, DateTime.UtcNow, false);
            Guid memberEmailId = await _memberEmailRepository.AddMemberEmail(memberEmail);

            return new MemberEmail(memberEmailId, memberEmail.ChapterId, memberEmail.ToAddress, memberEmail.Subject, memberEmail.CreatedDate, memberEmail.Sent);
        }

        public async Task<MemberEmail> SendMail(MemberEmail memberEmail, Member member, Email email)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(member.ChapterId);

            if (await Send(emailSettings.FromEmailAddress, member, email))
            {
                await _memberEmailRepository.ConfirmMemberEmailSent(memberEmail.Id);
            }

            return memberEmail;
        }

        public async Task<MemberEmail> SendMail(Member member, Email email, IDictionary<string, string> parameters)
        {
            MemberEmail memberEmail = await CreateMemberEmail(member, email, parameters);

            return await SendMail(memberEmail, member, email);
        }

        public async Task<MemberEmail> SendMail(Member member, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type);

            return await SendMail(member, email, parameters);
        }

        private static MimeMessage CreateMessage(string from, Member member, Email email)
        {
            MimeMessage message = new MimeMessage
            {
                Body = new TextPart(TextFormat.Html)
                {
                    Text = email.Body
                },
                Subject = email.Subject
            };

            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(member.EmailAddress));

            return message;
        }

        private async Task<bool> Send(string from, Member member, Email email)
        {
            MimeMessage message = CreateMessage(from, member, email);

            try
            {
                using SmtpClient client = new SmtpClient();
                await client.ConnectAsync(_smtpSettings.Host, 25, false);

                if (!string.IsNullOrWhiteSpace(_smtpSettings.Username) &&
                    !string.IsNullOrWhiteSpace(_smtpSettings.Password))
                {
                    client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
