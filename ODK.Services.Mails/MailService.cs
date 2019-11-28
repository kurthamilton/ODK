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
        private readonly MailServiceSettings _settings;

        public MailService(MailServiceSettings settings, IChapterRepository chapterRepository, IMemberEmailRepository memberEmailRepository)
        {
            _chapterRepository = chapterRepository;
            _memberEmailRepository = memberEmailRepository;
            _settings = settings;
        }

        public async Task<MemberEmail> CreateMemberEmail(Member member, Email email, IDictionary<string, string> parameters)
        {
            email = email.Interpolate(parameters);

            MemberEmail memberEmail = new MemberEmail(Guid.Empty, member.ChapterId, member.EmailAddress, email.Subject, DateTime.UtcNow, false);
            Guid memberEmailId = await _memberEmailRepository.AddMemberEmail(memberEmail);

            return new MemberEmail(memberEmailId, memberEmail.ChapterId, memberEmail.ToAddress, memberEmail.Subject, memberEmail.CreatedDate, memberEmail.Sent);
        }

        public async Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(EmailType.ContactRequest);

            email = email.Interpolate(parameters);

            ChapterEmailSettings emailSettings = await GetChapterEmailSettings(chapter.Id);

            MimeMessage message = CreateMessage(emailSettings.FromEmailAddress, emailSettings.ContactEmailAddress, email);

            await Send(message);
        }

        public async Task<MemberEmail> SendMemberMail(MemberEmail memberEmail, Member member, Email email)
        {
            ChapterEmailSettings emailSettings = await GetChapterEmailSettings(member.ChapterId);

            if (await Send(emailSettings.FromEmailAddress, member, email))
            {
                await _memberEmailRepository.ConfirmMemberEmailSent(memberEmail.Id);
            }

            return memberEmail;
        }

        public async Task<MemberEmail> SendMemberMail(Member member, Email email, IDictionary<string, string> parameters)
        {
            MemberEmail memberEmail = await CreateMemberEmail(member, email, parameters);

            return await SendMemberMail(memberEmail, member, email);
        }

        public async Task<MemberEmail> SendMemberMail(Member member, EmailType type, IDictionary<string, string> parameters)
        {
            Email email = await _memberEmailRepository.GetEmail(type);

            return await SendMemberMail(member, email, parameters);
        }

        private static MimeMessage CreateMessage(string from, Member member, Email email)
        {
            return CreateMessage(from, member.EmailAddress, email);
        }

        private static MimeMessage CreateMessage(string from, string to, Email email)
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
            message.To.Add(new MailboxAddress(to));

            return message;
        }

        private async Task<ChapterEmailSettings> GetChapterEmailSettings(Guid chapterId)
        {
            return await _chapterRepository.GetChapterEmailSettings(chapterId);
        }

        private async Task<bool> Send(string from, Member member, Email email)
        {
            MimeMessage message = CreateMessage(from, member, email);
            return await Send(message);
        }

        private async Task<bool> Send(MimeMessage message)
        {
            try
            {
                using SmtpClient client = new SmtpClient();
                await client.ConnectAsync(_settings.Host, 25, false);

                if (!string.IsNullOrWhiteSpace(_settings.Username) &&
                    !string.IsNullOrWhiteSpace(_settings.Password))
                {
                    client.Authenticate(_settings.Username, _settings.Password);
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
