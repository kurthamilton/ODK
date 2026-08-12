using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Tasks;
using ODK.Services.Web;

namespace ODK.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEmailClient _emailClient;
    private readonly ILoggingService _loggingService;
    private readonly EmailServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public EmailService(
        IUnitOfWork unitOfWork,
        IEmailClient emailClient,
        IUrlProviderFactory urlProviderFactory,
        IBackgroundTaskService backgroundTaskService,
        ILoggingService loggingService,
        EmailServiceSettings settings)
    {
        _backgroundTaskService = backgroundTaskService;
        _emailClient = emailClient;
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;
        _settings = settings;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task AddEvent(string externalId, string eventName)
    {
        var sentEmail = await _unitOfWork.SentEmailRepository.GetByExternalId(externalId).Run();
        if (sentEmail == null)
        {
            await _loggingService.Warn($"Sent email not found for externalId {externalId} when logging event");
            return;
        }

        _unitOfWork.SentEmailEventRepository.Add(new SentEmailEvent
        {
            CreatedUtc = DateTime.UtcNow,
            EventName = eventName,
            SentEmailId = sentEmail.Id
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        EmailType type,
        IEmailParameters? parameters)
    {
        var chapter = request.Chapter;

        await SendEmail(request, new SendEmailOptions
        {
            Body = string.Empty,
            Chapter = chapter,
            Parameters = parameters,
            Subject = string.Empty,
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
            Type = type
        });
    }

    public async Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        var chapter = request.Chapter;

        await SendEmail(request, new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = to.Select(x => x.ToEmailAddressee()).ToArray(),
        });
    }

    public async Task SendEventCommentEmail(
        IServiceRequest request,
        Chapter chapter,
        Member? replyToMember,
        EventComment comment,
        IEmailParameters? parameters)
    {
        var platform = request.Platform;

        var (chapterAdminMembers, replyToMemberEmailPreference) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => replyToMember != null
                ? x.MemberEmailPreferenceRepository.GetByMemberId(replyToMember.Id, MemberEmailPreferenceType.EventMessages)
                : new DefaultDeferredQuerySingleOrDefault<MemberEmailPreference>());

        /* One send per audience, each reading its own template: admins are told a comment was left on an
           event they run, the replied-to member that someone answered them. The parameters are the same
           either way - only the template differs, so a group customising one cannot change the other. */
        var adminAddressees = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveEventCommentEmails))
            .ToArray();

        // Nothing is queued when every admin has opted out, rather than an email addressed to nobody.
        if (adminAddressees.Length > 0)
        {
            await SendEmail(request, new SendEmailOptions
            {
                Body = string.Empty,
                Chapter = chapter,
                Parameters = parameters,
                Subject = string.Empty,
                To = adminAddressees,
                Type = EmailType.EventComment
            });
        }

        if (replyToMember != null && replyToMemberEmailPreference?.Disabled != true)
        {
            await SendEmail(request, new SendEmailOptions
            {
                Body = string.Empty,
                Chapter = chapter,
                Parameters = parameters,
                Subject = string.Empty,
                To = [replyToMember.ToEmailAddressee()],
                Type = EmailType.EventCommentReply
            });
        }
    }

    public Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        EmailType type,
        IEmailParameters? parameters)
        => SendEmail(request, chapter, [to], type, parameters);

    public async Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        EmailType type,
        IEmailParameters? parameters)
    {
        return await SendEmail(request, new SendEmailOptions
        {
            Body = string.Empty,
            Chapter = chapter,
            Subject = string.Empty,
            Parameters = parameters,
            To = to.ToArray(),
            Type = type
        });
    }

    public async Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body)
    {
        return await SendEmail(request, chapter, to, subject, body, parameters: null);
    }

    public async Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        IEmailParameters? parameters)
    {
        return await SendEmail(request, new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Parameters = parameters,
            Subject = subject,
            To = to.ToArray()
        });
    }

    public async Task<ServiceResult> SendMemberEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body,
        IEmailParameters? parameters)
    {
        return await SendEmail(request, new SendEmailOptions
        {
            Body = body,
            Chapter = chapter,
            Subject = subject,
            To = [to],
            Parameters = parameters
        });
    }

    // Public for Hangfire
    public async Task SendQueuedEmailTask(Guid queuedEmailId)
    {
        var (queuedEmail, recipients) = await _unitOfWork.RunAsync(
            x => x.QueuedEmailRepository.GetById(queuedEmailId),
            x => x.QueuedEmailRecipientRepository.GetByQueuedEmailId(queuedEmailId));

        var email = new EmailClientEmail
        {
            Body = queuedEmail.Body,
            From = new EmailAddressee(queuedEmail.FromEmailAddress, queuedEmail.FromName),
            ScheduledUtc = queuedEmail.SendAfterUtc,
            Subject = queuedEmail.Subject,
            To = recipients
                .Select(x => new EmailAddressee(x.EmailAddress, x.Name))
                .ToArray()
        };

        var result = await _emailClient.SendEmail(email);
        if (!result.Success)
        {
            throw new OdkServiceException($"Error sending queued email");
        }

        var sentUtc = DateTime.UtcNow;

        var sentEmails = recipients
            .Select(x => new SentEmail
            {
                Id = Guid.NewGuid(),
                ExternalId = result.ExternalId,
                SentUtc = sentUtc,
                Subject = queuedEmail.Subject,
                To = x.EmailAddress
            });

        _unitOfWork.SentEmailRepository.AddMany(sentEmails);
        _unitOfWork.QueuedEmailRecipientRepository.DeleteMany(recipients);
        _unitOfWork.QueuedEmailRepository.Delete(queuedEmail);

        await _unitOfWork.SaveChangesAsync();
    }

    private static IEnumerable<EmailAddressee> GetAddressees(IEnumerable<ChapterAdminMember> adminMembers)
    {
        foreach (var adminMember in adminMembers)
        {
            yield return adminMember.ToEmailAddressee();
        }
    }

    private async Task<IReadOnlyDictionary<string, string>> BuildParameters(
        IServiceRequest request,
        SendEmailOptions options,
        SiteEmailSettings siteSettings,
        ChapterEmailSettings? chapterEmailSettings,
        Email? bodyEmail)
    {
        var urlProvider = await _urlProviderFactory.Create(request);

        var core = new EmailParameters
        {
            GroupUrl = options.Chapter != null ? urlProvider.GroupUrl(options.Chapter) : null,
            GroupFullName = StringUtils.Coalesce(options.Chapter?.FullName, siteSettings.PlatformTitle),
            GroupName = StringUtils.Coalesce(
                options.Chapter?.GetDisplayName(request.Platform), siteSettings.PlatformTitle),
            PlatformUrl = urlProvider.BaseUrl(),
            ThemeBodyBackground = _settings.DefaultBodyBackground,
            ThemeBodyColor = _settings.DefaultBodyColor,
            ThemeHeaderBackground = StringUtils.Coalesce(
                options.Chapter?.ThemeBackground, _settings.DefaultHeaderBackground),
            ThemeHeaderColor = StringUtils.Coalesce(
                options.Chapter?.ThemeColor, _settings.DefaultHeaderColor)
        };

        var parameters = new Dictionary<string, string>(
            core.ToDictionary(), EmailParameterComparer.Default);

        // Merged over the core values rather than filling gaps in them: an email that supplies a
        // parameter of its own is the more specific answer and wins.
        var supplied = options.Parameters?.ToDictionary();
        if (supplied != null)
        {
            foreach (var (name, value) in supplied)
            {
                parameters[name] = value;
            }
        }

        /* Resolved after the merge and not before, because each is itself a template over the parameters
           above it. A group's own wording wins where it set any; Coalesce treats blank as unset, which is
           how a group that has never filled the form in inherits the site's. */
        var titles = new (string Name, string Value)[]
        {
            (EmailParameters.AdminTitleName,
                StringUtils.Coalesce(chapterEmailSettings?.AdminTitle, siteSettings.AdminTitle)),
            (EmailParameters.MemberTitleName,
                StringUtils.Coalesce(chapterEmailSettings?.MemberTitle, siteSettings.MemberTitle)),
            // Site-wide on purpose - see TitleName. A group is not offered an override of it.
            (EmailParameters.TitleName, siteSettings.Title)
        };

        foreach (var (name, value) in titles)
        {
            parameters[name] = value.Interpolate(parameters.AsReadOnly(), HttpUtility.HtmlEncode);
        }

        var body = !string.IsNullOrEmpty(options.Body)
            ? options.Body
            : bodyEmail?.HtmlContent ?? string.Empty;
        body = body.Interpolate(parameters.AsReadOnly(), HttpUtility.HtmlEncode);

        foreach (var htmlParameter in parameters.Where(x => x.Key.StartsWith(EmailParameters.HtmlPrefix)))
        {
            var parameterName = htmlParameter.Key[EmailParameters.HtmlPrefix.Length..];
            body = body.Interpolate(new Dictionary<string, string>
            {
                { parameterName, parameters[htmlParameter.Key] }
            });
        }

        parameters[EmailParameters.BodyName] = body;

        return parameters.AsReadOnly();
    }

    private async Task<ServiceResult> SendEmail(IServiceRequest request, SendEmailOptions options)
    {
        var platform = request.Platform;
        var chapterId = options.Chapter?.Id;

        var (emails, chapterEmails, siteSettings, chapterEmailSettings) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetAll(),
            x => chapterId != null
                ? x.ChapterEmailRepository.GetByChapterId(chapterId.Value)
                : new DefaultDeferredQueryMultiple<ChapterEmail>(),
            x => x.SiteEmailSettingsRepository.Get(platform),
            x => chapterId != null
                ? x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapterId.Value)
                : new DefaultDeferredQuerySingleOrDefault<ChapterEmailSettings>());

        /* Where a group has overridden a template, the wording is theirs but the recipient type stays the
           site's - an override says how the email reads, not who it is for. */
        var siteLayoutEmail = emails.First(x => x.Type == EmailType.Layout);
        var layoutEmail = chapterEmails.FirstOrDefault(x => x.Type == EmailType.Layout)
                ?.ToEmail(siteLayoutEmail.RecipientType)
            ?? siteLayoutEmail;

        // Null for the ad-hoc sends, which carry their own body and default Type to Layout.
        Email? bodyEmail = null;
        if (options.Type != EmailType.Layout)
        {
            var siteBodyEmail = emails.First(x => x.Type == options.Type);
            bodyEmail = chapterEmails.FirstOrDefault(x => x.Type == options.Type)
                    ?.ToEmail(siteBodyEmail.RecipientType)
                ?? siteBodyEmail;
        }

        var parameters = await BuildParameters(
            request, options, siteSettings, chapterEmailSettings, bodyEmail);

        var subject = !string.IsNullOrEmpty(options.Subject)
            ? options.Subject
            : bodyEmail?.Subject ?? string.Empty;

        var queuedEmail = _unitOfWork.QueuedEmailRepository.Add(new QueuedEmail
        {
            Body = layoutEmail.HtmlContent.Interpolate(parameters),
            ChapterId = chapterId,
            CreatedUtc = DateTime.UtcNow,
            FromEmailAddress = siteSettings.FromEmailAddress,
            FromName = siteSettings.FromName.Interpolate(parameters),
            Id = Guid.NewGuid(),
            Subject = subject.Interpolate(parameters)
        });

        foreach (var recipient in options.To)
        {
            _unitOfWork.QueuedEmailRecipientRepository.Add(new QueuedEmailRecipient
            {
                EmailAddress = recipient.Address,
                Id = Guid.NewGuid(),
                Name = recipient.Name,
                QueuedEmailId = queuedEmail.Id
            });
        }

        await _unitOfWork.SaveChangesAsync();

        _backgroundTaskService.Enqueue(
            () => SendQueuedEmailTask(queuedEmail.Id),
            BackgroundTaskQueueType.Emails);

        return ServiceResult.Successful();
    }
}