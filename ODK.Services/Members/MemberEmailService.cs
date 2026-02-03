using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Payments;
using ODK.Core.Topics;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Web;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public MemberEmailService(
        IEmailService emailService,
        IUrlProviderFactory urlProviderFactory,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task SendActivationEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string token)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.ActivateAccountUrl(chapter, token);

        var to = member.ToEmailAddressee();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(request, chapter, to, EmailType.ActivateAccount, parameters);
    }

    public async Task SendAddressUpdateEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string newEmailAddress,
        string token)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.ConfirmEmailAddressUpdate(chapter, token);

        var to = new EmailAddressee(newEmailAddress, member.FullName);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(request, chapter, to, EmailType.EmailAddressUpdate, parameters);
    }

    public async Task SendBulkEmail(
        ServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        await _emailService.SendBulkEmail(
            request,
            chapter,
            to,
            subject,
            body);
    }

    public async Task SendChapterConversationEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply)
    {
        var subject = "{conversation.subject} - {title}";

        if (isReply)
        {
            subject = $"Re: {subject}";
        }

        var body = new EmailBodyBuilder()
            .AddParagraph("{conversation.message}")
            .AddParagraphLink("url")
            .ToString();

        var isToMember = message.MemberId != conversation.MemberId;

        if (isToMember)
        {
            var memberEmailPreference = await _unitOfWork.MemberEmailPreferenceRepository
                .GetByMemberId(conversation.MemberId, MemberEmailPreferenceType.ConversationMessages)
                .Run();

            if (memberEmailPreference?.Disabled == true)
            {
                return;
            }
        }

        var urlProvider = await _urlProviderFactory.Create(request);
        var url = isToMember
            ? urlProvider.ConversationUrl(chapter, conversation.Id)
            : urlProvider.ConversationAdminUrl(chapter, conversation.Id);

        var addressees = to.Select(x => x.ToEmailAddressee());

        var parameters = new Dictionary<string, string>
        {
            { "conversation.subject", conversation.Subject },
            { "conversation.message", message.Text },
            { "url", url }
        };

        await _emailService.SendEmail(request, chapter, addressees, subject, body, parameters);
    }

    public async Task SendChapterMessage(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        ChapterContactMessage message)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.MessageAdminUrl(chapter, message.Id);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "message.from", message.FromAddress },
            { "message.text", message.Message },
            { "url", url }
        };

        var to = adminMembers
            .Where(x => x.ReceiveContactEmails)
            .Select(x => x.ToEmailAddressee());

        await _emailService.SendEmail(
            request,
            chapter,
            to,
            EmailType.ContactRequest,
            parameters);
    }

    public async Task<ServiceResult> SendChapterMessageReply(
        ServiceRequest request,
        Chapter chapter,
        ChapterContactMessage originalMessage,
        string reply)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.GroupUrl(chapter);

        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, string.Empty)
        };

        var body = new EmailBodyBuilder()
            .AddText(reply)
            .AddLine()
            .AddParagraph("Your original message:")
            .AddText(originalMessage.Message)
            .AddParagraphLink("url")
            .ToString();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        return await _emailService.SendEmail(
            request,
            chapter,
            to,
            "Re: your message to {title}",
            body,
            parameters);
    }

    public async Task SendDuplicateMemberEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member)
    {
        var urlProvider = await _urlProviderFactory.Create(request);

        var url = urlProvider.LoginUrl(chapter);

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.DuplicateEmail,
            new Dictionary<string, string>
            {
                { "loginUrl", url  }
            });
    }

    public async Task SendEventCommentEmail(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.EventUrl(chapter, @event.Shortcode);

        var parameters = new Dictionary<string, string>
        {
            { "comment.text", eventComment.Text },
            { "event.id", @event.Id.ToString() },
            { "event.url", url }
        };

        await _emailService.SendEventCommentEmail(
            request,
            chapter,
            parentCommentMember,
            eventComment,
            parameters);
    }

    public async Task SendEventInvites(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members)
    {
        var time = @event.ToLocalTimeString(chapter.TimeZone);

        var urlProvider = await _urlProviderFactory.Create(request);
        var eventUrl = urlProvider.EventUrl(chapter, @event.Shortcode);
        var rsvpUrl = @event.Ticketed
            ? urlProvider.EventUrl(chapter, @event.Shortcode)
            : urlProvider.EventRsvpUrl(chapter, @event.Shortcode);
        var unsubscribeUrl = urlProvider.EmailPreferences(chapter);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "event.date", @event.Date.ToString("dddd dd MMMM, yyyy") },
            { "event.id", @event.Id.ToString() },
            { "event.location", venue.Name },
            { "event.name", @event.GetDisplayName() },
            { "event.time", time },
            { "event.rsvpurl", rsvpUrl },
            { "event.url", eventUrl },
            { "unsubscribeUrl", unsubscribeUrl }
        };

        await _emailService.SendBulkEmail(
            request,
            chapter,
            members,
            EmailType.EventInvite,
            parameters);
    }

    public async Task SendEventWaitlistPromotionNotification(
        ServiceRequest request,
        Chapter chapter,
        Event @event,
        IEnumerable<Member> members)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.EventUrl(chapter, @event.Shortcode);

        var subject = "{title} - You're in! A spot opened up for {event.name}";

        var body = new EmailBodyBuilder()
            .AddParagraph("A spot has opened up for {event.name} on {event.date}.")
            .AddParagraph("Please update your RSVP if you no longer wish to attend.")
            .AddParagraphLink("url")
            .ToString();

        var parameters = new Dictionary<string, string>
        {
            { "url", url },
            { "event.date", @event.Date.ToString("dddd dd MMMM, yyyy") },
            { "event.name", @event.GetDisplayName() }
        };

        var to = members
            .Select(x => x.ToEmailAddressee())
            .ToArray();

        await _emailService.SendEmail(
            request,
            chapter,
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendGroupApprovedEmail(
        ServiceRequest request,
        Chapter chapter,
        Member owner)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.GroupUrl(chapter);

        var subject = "{title} - Your group has been approved 🚀";

        var body = new EmailBodyBuilder()
            .AddParagraph("Your group <strong>{chapter.fullName}</strong> has been approved and you are ready to go!")
            .AddParagraphLink("url")
            .ToString();

        var to = owner.ToEmailAddressee();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendMemberEmail(
            request,
            chapter,
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendMemberApprovedEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.GroupUrl(chapter);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        var subject = "{title} - You have been approved by {chapter.fullName}";
        var body = new EmailBodyBuilder()
            .AddParagraph("Your application to join {chapter.fullName} has been approved")
            .AddParagraphLink("url")
            .ToString();

        await _emailService.SendMemberEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            subject,
            body,
            parameters);
    }

    public async Task SendIssueReply(
        ServiceRequest request,
        Issue issue,
        IssueMessage reply,
        Member? toMember,
        SiteEmailSettings siteEmailSettings)
    {
        var subject = "Re: {issue.title} - issue updated - {title}";

        var isToMember = toMember != null;

        var bodyBuilder = new EmailBodyBuilder();

        if (isToMember)
        {
            bodyBuilder
                .AddParagraph("Your issue {issue.title} has been updated with the following message:");
        }
        else
        {
            bodyBuilder
                .AddParagraph("The owner of the issue {issue.title} has sent the following message:");
        }

        var body = bodyBuilder
            .AddParagraph("{issue.message}")
            .AddParagraphLink("url")
            .ToString();

        var urlProvider = await _urlProviderFactory.Create(request);
        var url = isToMember
            ? urlProvider.IssueUrl(issue.Id)
            : urlProvider.IssueAdminUrl(issue.Id);

        var to = toMember?.ToEmailAddressee() ?? new EmailAddressee(siteEmailSettings.ContactEmailAddress, string.Empty);

        var parameters = new Dictionary<string, string>
        {
            { "issue.title", issue.Title },
            { "issue.message", reply.Text },
            { "url", url }
        };

        await _emailService.SendEmail(
            request,
            null,
            [to],
            subject,
            body,
            parameters);
    }

    public async Task SendMemberChapterSubscriptionConfirmationEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc)
    {
        var currency = chapterSubscription.Currency;

        var parameters = new Dictionary<string, string>
        {
            { "subscription.amount", currency.ToAmountString(chapterSubscription.Amount) },
            { "subscription.end", chapter.ToChapterTime(expiresUtc).ToString("d MMMM yyyy") }
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.SubscriptionConfirmation,
            parameters);
    }

    public async Task SendMemberChapterSubscriptionExpiringEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member,
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate)
    {
        var expiring = expires > DateTime.UtcNow;

        var properties = new Dictionary<string, string>
        {
            { "member.firstName", member.FirstName },
            { "subscription.expiryDate", expires.ToFriendlyDateString(chapter.TimeZone) },
            { "subscription.disabledDate", disabledDate.ToFriendlyDateString(chapter.TimeZone) }
        };

        var emailType = expiring
            ? memberSubscription.Type switch
            {
                SubscriptionType.Trial => EmailType.TrialExpiring,
                _ => EmailType.SubscriptionExpiring
            }
            : memberSubscription.Type switch
            {
                SubscriptionType.Trial => EmailType.TrialExpired,
                _ => EmailType.SubscriptionExpired
            };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            emailType,
            properties);
    }

    public async Task SendMemberDeleteEmail(
        ServiceRequest request,
        Chapter chapter,
        Member member,
        string? reason)
    {
        var subject = "{title} - you have been removed from a group";

        var bodyBuilder = new EmailBodyBuilder()
            .AddParagraph("You have been removed from the {chapter.fullName} group");

        if (!string.IsNullOrEmpty(reason))
        {
            bodyBuilder
                .AddParagraph("The following reason was given:")
                .AddParagraph("{reason}");
        }

        var body = bodyBuilder.ToString();

        var parameters = new Dictionary<string, string>
        {
            { "reason", reason ?? string.Empty }
        };

        await _emailService.SendEmail(
            request,
            chapter,
            [member.ToEmailAddressee()],
            subject,
            body,
            parameters);
    }

    public async Task SendMemberLeftChapterEmail(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        string? reason)
    {
        var emailRecipients = adminMembers
            .Where(x => x.MemberId != member.Id && x.ReceiveNewMemberEmails)
            .Select(x => x.ToEmailAddressee())
            .ToArray();

        if (emailRecipients.Length == 0)
        {
            return;
        }

        var subject = "{title} - {member.name} has left {chapter.fullName}";

        var bodyBuilder = new EmailBodyBuilder()
            .AddParagraph("{member.name} has left {chapter.fullName}")
            .AddParagraph("They had been a member since {joined}");

        if (!string.IsNullOrEmpty(reason))
        {
            bodyBuilder
                .AddParagraph("They gave the following reason:")
                .AddParagraph("{reason}");
        }
        else
        {
            bodyBuilder.AddParagraph("They did not give a reason");
        }

        var body = bodyBuilder.ToString();

        var memberChapter = member.MemberChapter(chapter.Id);

        var parameters = new Dictionary<string, string>
        {
            { "member.name", member.FullName },
            { "joined", memberChapter?.CreatedUtc.ToFriendlyDateString(chapter.TimeZone) ?? "-" },
            { "reason", reason ?? string.Empty }
        };

        await _emailService.SendEmail(
            request,
            chapter,
            emailRecipients,
            subject: subject,
            body: body,
            parameters: parameters);
    }

    public async Task SendNewGroupEmail(
        ServiceRequest request,
        Chapter chapter,
        ChapterTexts texts,
        SiteEmailSettings settings)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.SiteAdminGroups();

        var parameters = new Dictionary<string, string>
        {
            { "chapter.description", texts.Description ?? string.Empty },
            { "url", url }
        };

        var to = new EmailAddressee(settings.ContactEmailAddress, string.Empty);

        var subject = "{title} - New group";

        var body = new EmailBodyBuilder()
            .AddParagraph("A group has just been created")
            .AddParagraph("Name: {chapter.fullName}")
            .AddParagraph("{chapter.description}")
            .AddParagraphLink("url")
            .ToString();

        await _emailService.SendMemberEmail(
            request,
            chapter: null,
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendNewIssueEmail(
        ServiceRequest request,
        Member member,
        Issue issue,
        IssueMessage message,
        SiteEmailSettings settings)
    {
        var subject = "{title} - New issue";

        var body = new EmailBodyBuilder()
            .AddParagraph("A new issue has been created by {member.name}:")
            .AddParagraph("{issue.title}")
            .AddParagraph("{issue.message}")
            .AddParagraphLink("url")
            .ToString();

        var urlProvider = await _urlProviderFactory.Create(request);
        var to = new EmailAddressee(settings.ContactEmailAddress, string.Empty);
        await _emailService.SendMemberEmail(
            request,
            null,
            to,
            subject,
            body,
            new Dictionary<string, string>
            {
                { "issue.title", issue.Title },
                { "member.name", member.FullName },
                { "url", urlProvider.IssueAdminUrl(issue.Id) }
            });
    }

    public async Task SendNewMemberAdminEmail(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var memberPropertyDictionary = memberProperties
            .ToDictionary(x => x.ChapterPropertyId);

        var memberPropertiesBuilder = new EmailTableBuilder()
            .AddRow("Name", member.FullName);

        foreach (var chapterProperty in chapterProperties.Where(x => !x.ApplicationOnly).OrderBy(x => x.DisplayOrder))
        {
            memberPropertyDictionary.TryGetValue(chapterProperty.Id, out var memberProperty);

            memberPropertiesBuilder.AddRow(
                chapterProperty.Label,
                memberProperty?.Value ?? "-");
        }

        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.MemberAdminUrl(chapter, member.Id);

        var parameters = new Dictionary<string, string>
        {
            { "html:member.properties", memberPropertiesBuilder.ToString() },
            { "url", url }
        };

        var to = adminMembers
            .Where(x => x.ReceiveNewMemberEmails)
            .Select(x => x.ToEmailAddressee())
            .ToArray();

        await _emailService.SendEmail(
            request,
            chapter,
            to,
            EmailType.NewMemberAdmin,
            parameters);
    }

    public async Task SendNewMemberEmailsAsync(
        ServiceRequest request,
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var eventsUrl = urlProvider.EventsUrl(chapter);

        var parameters = new Dictionary<string, string>
        {
            { "eventsUrl", eventsUrl },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) }
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.NewMember,
            parameters);

        await SendNewMemberAdminEmail(
            request,
            chapter,
            adminMembers,
            member,
            chapterProperties,
            memberProperties);
    }

    public async Task SendNewTopicEmail(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        SiteEmailSettings settings)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.TopicApprovalUrl();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        var subject = "{title} - New topics";

        var tableBuilder = new EmailTableBuilder();
        for (var i = 0; i < newTopics.Count; i++)
        {
            tableBuilder.AddRow($"{{topicgroup-{i}}}", $"{{topic-{i}}}");

            var newTopic = newTopics.ElementAt(i);
            parameters.Add($"topicgroup-{i}", newTopic.TopicGroup);
            parameters.Add($"topic-{i}", newTopic.Topic);
        }

        var body = new EmailBodyBuilder()
            .AddParagraph("The following topics require approval")
            .AddTable(tableBuilder)
            .AddParagraphLink("url")
            .ToString();

        var to = new EmailAddressee(settings.ContactEmailAddress, string.Empty);

        await _emailService.SendMemberEmail(
            request,
            null,
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendPasswordResetEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member member,
        string token)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.PasswordReset(chapter, token);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.PasswordReset,
            parameters);
    }

    public async Task SendPaymentNotification(
        ServiceRequest request,
        Member member,
        Chapter? chapter,
        Payment payment,
        Currency currency,
        SiteEmailSettings settings)
    {
        var to = new EmailAddressee(settings.ContactEmailAddress, string.Empty);
        var subject = "{title} - Payment Received";
        var body =
            $"<p>A payment has been received for {currency.ToAmountString(payment.Amount)}.</p>" +
            $"<p>Reference: {payment.Reference}</p>";

        await _emailService.SendEmail(
            request,
            chapter,
            [to],
            subject,
            body);

        to = new EmailAddressee(member.EmailAddress, member.FullName);
        body =
            $"<p>Your payment of {currency.ToAmountString(payment.Amount)} has been processed.</p>" +
            $"<p>Reference: {payment.Reference}</p>";

        await _emailService.SendEmail(
            request,
            null,
            [to],
            subject,
            body);
    }

    public async Task SendSiteMessage(
        ServiceRequest request,
        SiteContactMessage message,
        SiteEmailSettings settings)
    {
        var urlProvider = await _urlProviderFactory.Create(request);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "message.from", message.FromAddress },
            { "message.text", message.Message },
            { "url", urlProvider.MessageSiteAdminUrl(message.Id) }
        };

        var to = new EmailAddressee(settings.ContactEmailAddress, string.Empty);

        await _emailService.SendEmail(
            request,
            null,
            to,
            EmailType.ContactRequest,
            parameters);
    }

    public async Task<ServiceResult> SendSiteMessageReply(
        ServiceRequest request,
        SiteContactMessage originalMessage,
        string reply)
    {
        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, string.Empty)
        };

        var subject = "Re: your message to {title}";

        var body = new EmailBodyBuilder()
            .AddText(reply)
            .AddLine()
            .AddParagraph("Your original message:")
            .AddText(originalMessage.Message)
            .ToString();

        return await _emailService.SendEmail(
            request,
            null,
            to,
            subject,
            body);
    }

    public async Task SendSiteSubscriptionExpiredEmail(
        ServiceRequest request,
        Member member)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.MemberSiteSubscriptionUrl();

        var subject = "{title} - Subscription Expired";
        var body = new EmailBodyBuilder()
            .AddParagraph("Your subscription has now expired")
            .AddParagraphLink("url")
            .ToString();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendMemberEmail(
            request,
            null,
            member.ToEmailAddressee(),
            subject,
            body);
    }

    public async Task SendSiteWelcomeEmail(
        ServiceRequest request,
        Member member)
    {
        var urlProvider = await _urlProviderFactory.Create(request);
        var url = urlProvider.GroupsUrl();

        var subject = "{title} - Welcome!";

        var body = new EmailBodyBuilder()
            .AddParagraph("Welcome to {title} {member.firstName}!")
            .AddParagraph("Enjoy creating or joining your first group, and please do share.")
            .AddParagraphLink("url")
            .ToString();

        var parameters = new Dictionary<string, string>
        {
            { "member.firstName", member.FirstName },
            { "url", url }
        };

        await _emailService.SendMemberEmail(
            request,
            null,
            member.ToEmailAddressee(),
            subject,
            body,
            parameters);
    }

    public async Task<ServiceResult> SendTestEmail(
        ServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type)
    {
        var parameters = new Dictionary<string, string>
        {
            { "member.emailAddress", to.FirstName },
            { "member.firstName", to.FirstName },
            { "member.lastName", to.FirstName }
        };

        return await _emailService.SendEmail(
            request,
            chapter,
            to.ToEmailAddressee(),
            type,
            parameters);
    }

    public async Task SendTopicApprovedEmails(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members)
    {
        if (newTopics.Count == 0)
        {
            return;
        }

        var memberDictionary = members.ToDictionary(x => x.Id);
        var newTopicDictionary = newTopics
            .Where(x => memberDictionary.ContainsKey(x.MemberId))
            .GroupBy(x => x.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var member in members)
        {
            newTopicDictionary.TryGetValue(member.Id, out var memberTopics);

            if (memberTopics == null || memberTopics.Length == 0)
            {
                continue;
            }

            var subject = $"{{title}} - {StringUtils.Pluralise(memberTopics.Length, "Topic")} approved";

            var parameters = new Dictionary<string, string>();

            var topicTableBuilder = new EmailTableBuilder();
            for (var i = 0; i < memberTopics.Length; i++)
            {
                var topicGroupParam = $"topicgroup-{i}";
                var topicParam = $"topic-{i}";

                topicTableBuilder.AddRow($"{{{topicGroupParam}}}", $"{{{topicParam}}}");

                var memberTopic = memberTopics.ElementAt(i);
                parameters.Add(topicGroupParam, memberTopic.TopicGroup);
                parameters.Add(topicParam, memberTopic.Topic);
            }

            var message =
                $"The following {StringUtils.Pluralise(memberTopics.Length, "topic")} " +
                $"{(memberTopics.Length == 1 ? "has" : "have")} been approved";

            var body = new EmailBodyBuilder()
                .AddParagraph(message)
                .AddTable(topicTableBuilder)
                .ToString();

            await _emailService.SendMemberEmail(
                request,
                null,
                member.ToEmailAddressee(),
                subject,
                body,
                parameters);
        }
    }

    public async Task SendTopicRejectedEmails(
        ServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members)
    {
        if (newTopics.Count == 0)
        {
            return;
        }

        var memberDictionary = members.ToDictionary(x => x.Id);
        var newTopicDictionary = newTopics
            .Where(x => memberDictionary.ContainsKey(x.MemberId))
            .GroupBy(x => x.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var member in members)
        {
            newTopicDictionary.TryGetValue(member.Id, out var memberTopics);

            if (memberTopics == null || memberTopics.Length == 0)
            {
                continue;
            }

            var subject = $"{{title}} - {StringUtils.Pluralise(memberTopics.Length, "Topic")} rejected";

            var parameters = new Dictionary<string, string>();

            var topicTableBuilder = new EmailTableBuilder();
            for (var i = 0; i < memberTopics.Length; i++)
            {
                var topicGroupParam = $"topicgroup-{i}";
                var topicParam = $"topic-{i}";

                topicTableBuilder.AddRow($"{{{topicGroupParam}}}", $"{{{topicParam}}}");

                var memberTopic = memberTopics.ElementAt(i);
                parameters.Add(topicGroupParam, memberTopic.TopicGroup);
                parameters.Add(topicParam, memberTopic.Topic);
            }

            var message =
                $"The following {StringUtils.Pluralise(memberTopics.Length, "topic")} " +
                $"{(memberTopics.Length == 1 ? "has" : "have")} been rejected";

            var body = new EmailBodyBuilder()
                .AddParagraph(message)
                .AddTable(topicTableBuilder)
                .ToString();

            await _emailService.SendMemberEmail(
                request,
                null,
                member.ToEmailAddressee(),
                subject,
                body,
                parameters);
        }
    }
}