using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Services.Emails.Extensions;
using ODK.Services.Exceptions;
using ODK.Services.Logging;

namespace ODK.Services.Emails;

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

    public async Task SendBulkEmail(Chapter chapter, IEnumerable<EmailAddressee> to, string subject, string body, bool bcc = true)
    {
        await SendBulkEmail(chapter, to, subject, body, null, bcc);
    }

    public async Task SendBulkEmail(Chapter chapter, IEnumerable<EmailAddressee> to, string subject, string body, ChapterAdminMember? from, 
        bool bcc = true)
    {
        body = await GetLayoutBody(chapter, body);

        int i = 0;
        List<EmailAddressee> toList = to.ToList();
        while (i < toList.Count)
        {
            (ChapterEmailProvider provider, int remaining) = await GetProvider(chapter.Id);

            int batchSize = provider.BatchSize ?? (toList.Count - i);
            if (batchSize > remaining)
            {
                batchSize = remaining;
            }

            IEnumerable<EmailAddressee> batch = toList.Skip(i).Take(batchSize);
            MimeMessage message = await CreateMessage(provider, from, subject, body);
            if (bcc)
            {
                AddBulkEmailBccRecipients(message, message.From.First(), batch);
            }
            else
            {
                AddBulkEmailRecipients(message, batch);
            }

            await SendEmail(provider, message);
            i += batchSize;
        }
    }

    public async Task<ServiceResult> SendEmail(Chapter chapter, EmailAddressee to, string subject, string body)
    {
        return await SendEmail(chapter, to, subject, body, null);
    }

    public async Task<ServiceResult> SendEmail(Chapter chapter, EmailAddressee to, string subject, string body, 
        ChapterAdminMember? from)
    {
        body = await GetLayoutBody(chapter, body);

        (ChapterEmailProvider provider, int _) = await GetProvider(chapter.Id);

        MimeMessage message = await CreateMessage(provider, from, subject, body);
        AddEmailRecipient(message, to.ToMailboxAddress());

        return await SendEmail(provider, message);
    }

    private static void AddBulkEmailRecipients(MimeMessage message, IEnumerable<EmailAddressee> recipients)
    {
        foreach (EmailAddressee recipient in recipients)
        {
            message.To.Add(recipient.ToMailboxAddress());
        }
    }

    private static void AddBulkEmailBccRecipients(MimeMessage message, InternetAddress to, IEnumerable<EmailAddressee> recipients)
    {
        AddEmailRecipient(message, to);

        foreach (EmailAddressee recipient in recipients)
        {
            message.Bcc.Add(recipient.ToMailboxAddress());
        }
    }

    private static void AddEmailRecipient(MimeMessage message, InternetAddress to)
    {
        message.To.Add(to);
    }

    private async Task AddEmailFrom(MimeMessage message, ChapterEmailProvider provider, ChapterAdminMember? from)
    {
        if (from != null)
        {
            Member? member = await _memberRepository.GetMember(from.MemberId);
            message.From.Add(new MailboxAddress($"{member!.FirstName} {member.LastName}", from.AdminEmailAddress));
        }
        else
        {
            message.From.Add(new MailboxAddress(provider.FromName, provider.FromEmailAddress));
        }
    }

    private async Task<MimeMessage> CreateMessage(ChapterEmailProvider provider, ChapterAdminMember? from, string subject, string body)
    {
        MimeMessage message = new MimeMessage
        {
            Body = new TextPart(TextFormat.Html)
            {
                Text = body
            },
            Subject = subject
        };

        await AddEmailFrom(message, provider, from);

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

    private async Task<(ChapterEmailProvider Provider, int Remaining)> GetProvider(Guid chapterId)
    {
        IReadOnlyCollection<ChapterEmailProvider> providers = await _chapterRepository.GetChapterEmailProviders(chapterId);
        foreach (ChapterEmailProvider provider in providers)
        {
            int sentToday = await _emailRepository.GetEmailsSentTodayCount(provider.Id);
            if (sentToday < provider.DailyLimit)
            {
                return (provider, provider.DailyLimit - sentToday);
            }
        }

        throw new OdkServiceException("No more emails can be sent today");
    }

    private async Task<ServiceResult> SendEmail(ChapterEmailProvider provider, MimeMessage message)
    {
        if (message.To.Count == 0)
        {
            await _loggingService.LogDebug("Not sending email, no recipients set");
            return ServiceResult.Failure("No recipients set");
        }

        await _loggingService.LogDebug($"Sending email to {string.Join(", ", message.To)}");

        try
        {
            using SmtpClient client = new SmtpClient
            {
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };
            await client.ConnectAsync(provider.SmtpServer, provider.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable);
            await client.AuthenticateAsync(provider.SmtpLogin, provider.SmtpPassword);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            foreach (InternetAddress to in message.To.Union(message.Cc).Union(message.Bcc))
            {
                SentEmail sentEmail = new SentEmail(provider.Id, DateTime.UtcNow, to.ToString(), message.Subject);
                await _emailRepository.AddSentEmail(sentEmail);
            }

            await _loggingService.LogDebug($"Email sent to {string.Join(", ", message.To)}");
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            await _loggingService.LogError(ex, new Dictionary<string, string>
            {
                { "MAIL.TO", string.Join(", ", message.To) },
                { "MAIL.HTMLBODY", message.HtmlBody },
                { "MAIL.SUBJECT", message.Subject }
            });
            return ServiceResult.Failure($"Error sending email: {ex.Message}");
        }
    }
}
