using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Services.Exceptions;
using ODK.Services.Logging;

namespace ODK.Services.Emails
{
    public class MailProvider : IMailProvider
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly ILoggingService _loggingService;
        private readonly IMemberRepository _memberRepository;

        public MailProvider(IChapterRepository chapterRepository, IMemberRepository memberRepository, ILoggingService loggingService,
            IEmailRepository emailRepository)
        {
            _chapterRepository = chapterRepository;
            _emailRepository = emailRepository;
            _loggingService = loggingService;
            _memberRepository = memberRepository;
        }

        public async Task SendBulkEmail(Chapter chapter, IEnumerable<string> to, string subject, string body, bool bcc = true)
        {
            await SendBulkEmail(chapter, to, subject, body, null, bcc);
        }

        public async Task SendBulkEmail(Chapter chapter, IEnumerable<string> to, string subject, string body, ChapterAdminMember from, bool bcc = true)
        {
            body = await GetLayoutBody(chapter, body);

            ChapterEmailProviderSettings settings = await _chapterRepository.GetChapterEmailProviderSettings(chapter.Id);

            const int batchSize = 90;
            int i = 0;
            List<string> toList = to.ToList();
            while (i < toList.Count)
            {
                IEnumerable<string> batch = toList.Skip(i).Take(batchSize);
                MimeMessage message = await CreateMessage(settings, from, subject, body);
                if (bcc)
                {
                    AddBulkEmailBccRecipients(message, message.From.First(), batch);
                }
                else
                {
                    AddBulkEmailRecipients(message, batch);
                }

                await SendEmail(settings, message);
                i += batchSize;
            }
        }

        public async Task SendEmail(Chapter chapter, string to, string subject, string body)
        {
            await SendEmail(chapter, to, subject, body, null);
        }

        public async Task SendEmail(Chapter chapter, string to, string subject, string body, ChapterAdminMember from)
        {
            body = await GetLayoutBody(chapter, body);

            ChapterEmailProviderSettings settings = await _chapterRepository.GetChapterEmailProviderSettings(chapter.Id);

            MimeMessage message = await CreateMessage(settings, from, subject, body);
            AddEmailRecipient(message, new MailboxAddress(to));

            await SendEmail(settings, message);
        }

        private static void AddBulkEmailRecipients(MimeMessage message, IEnumerable<string> recipients)
        {
            foreach (string recipient in recipients)
            {
                message.To.Add(new MailboxAddress(recipient));
            }
        }

        private static void AddBulkEmailBccRecipients(MimeMessage message, InternetAddress to, IEnumerable<string> recipients)
        {
            AddEmailRecipient(message, to);

            foreach (string recipient in recipients)
            {
                message.Bcc.Add(new MailboxAddress(recipient));
            }
        }

        private static void AddEmailRecipient(MimeMessage message, InternetAddress to)
        {
            message.To.Add(to);
        }

        private async Task AddEmailFrom(MimeMessage message, ChapterEmailProviderSettings settings, ChapterAdminMember from)
        {
            if (from != null)
            {
                Member member = await _memberRepository.GetMember(from.MemberId);
                message.From.Add(new MailboxAddress($"{member.FirstName} {member.LastName}", from.AdminEmailAddress));
            }
            else
            {
                message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmailAddress));
            }
        }

        private async Task<MimeMessage> CreateMessage(ChapterEmailProviderSettings settings, ChapterAdminMember from, string subject, string body)
        {
            MimeMessage message = new MimeMessage
            {
                Body = new TextPart(TextFormat.Html)
                {
                    Text = body
                },
                Subject = subject
            };

            await AddEmailFrom(message, settings, from);

            return message;
        }

        private async Task<string> GetLayoutBody(Chapter chapter, string body)
        {
            Email layout = await _emailRepository.GetEmail(EmailType.Layout, chapter.Id);

            layout = layout.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "body", body }
            });

            return layout.HtmlContent;
        }

        private async Task SendEmail(ChapterEmailProviderSettings settings, MimeMessage message)
        {
            if (message.To.Count == 0)
            {
                await _loggingService.LogDebug("Not sending email, no recipients set");
                return;
            }

            await _loggingService.LogDebug($"Sending email to {string.Join(", ", message.To)}");

            try
            {
                using SmtpClient client = new SmtpClient
                {
                    ServerCertificateValidationCallback = (s, c, h, e) => true
                };
                await client.ConnectAsync(settings.SmtpServer, settings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable);
                client.Authenticate(settings.SmtpLogin, settings.SmtpPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                await _loggingService.LogDebug($"Email sent to {string.Join(", ", message.To)}");
            }
            catch (Exception ex)
            {
                await _loggingService.LogError(ex, "Error sending email");
                throw new OdkServiceException("Error sending email");
            }
        }
    }
}
