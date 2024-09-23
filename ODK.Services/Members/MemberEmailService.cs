using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Topics;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public MemberEmailService(
        IEmailService emailService, 
        IUrlProvider urlProvider,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task SendActivationEmail(Chapter? chapter, Member member, string token)
    {        
        var url = _urlProvider.ActivateAccountUrl(chapter, token);

        var to = member.ToEmailAddressee();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(chapter, to, EmailType.ActivateAccount, parameters);
    }

    public async Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token)
    {
        var url = _urlProvider.ConfirmEmailAddressUpdate(chapter, token);

        var to = new EmailAddressee(newEmailAddress, member.FullName);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(chapter, to, EmailType.EmailAddressUpdate, parameters);
    }

    public async Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        await _emailService.SendBulkEmail(
            chapter,
            to,
            subject,
            body);
    }

    public async Task SendChapterConversationEmail(
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

        var url = isToMember
            ? _urlProvider.ConversationUrl(chapter, conversation.Id)
            : _urlProvider.ConversationAdminUrl(chapter, conversation.Id);

        var addressees = to.Select(x => x.ToEmailAddressee());

        var parameters = new Dictionary<string, string>
        {
            { "conversation.subject", conversation.Subject },
            { "conversation.message", message.Text },
            { "url", url }
        };

        await _emailService.SendEmail(chapter, addressees, subject, body, parameters);
    }

    public async Task SendChapterMessage(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        ChapterContactMessage message)
    {
        var url = _urlProvider.MessageAdminUrl(chapter, message.Id);

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
            chapter,
            to,
            EmailType.ContactRequest,
            parameters);
    }

    public async Task<ServiceResult> SendChapterMessageReply(
        Chapter chapter, 
        ChapterContactMessage originalMessage, 
        string reply)
    {
        var url = _urlProvider.GroupUrl(chapter);

        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, "")
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

        return await _emailService.SendEmail(chapter, to, "Re: your message to {title}", body, parameters);
    }

    public async Task SendDuplicateMemberEmail(Chapter? chapter, Member member)
    {
        await _emailService.SendEmail(
            chapter,
            member.ToEmailAddressee(),
            EmailType.DuplicateEmail,
            new Dictionary<string, string>());
    }

    public async Task SendEventCommentEmail(
        Chapter chapter, 
        Event @event, 
        EventComment eventComment, 
        Member? parentCommentMember)
    {
        var url = _urlProvider.EventUrl(chapter, @event.Id);

        var parameters = new Dictionary<string, string>
        {
            { "comment.text", eventComment.Text },
            { "event.id", @event.Id.ToString() },
            { "event.url", url }
        };

        await _emailService.SendEventCommentEmail(chapter, parentCommentMember, eventComment, parameters);
    }

    public async Task SendEventInvites(
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members)
    {
        var time = @event.ToLocalTimeString(chapter.TimeZone);

        var eventUrl = _urlProvider.EventUrl(chapter, @event.Id);
        var rsvpUrl = @event.Ticketed 
            ? _urlProvider.EventUrl(chapter, @event.Id)
            : _urlProvider.EventRsvpUrl(chapter, @event.Id);
        var unsubscribeUrl = _urlProvider.EmailPreferences(chapter);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "event.date", @event.Date.ToString("dddd dd MMMM, yyyy") },
            { "event.id", @event.Id.ToString() },
            { "event.location", venue.Name },
            { "event.name", @event.Name },
            { "event.time", time },
            { "event.rsvpurl", rsvpUrl },
            { "event.url", eventUrl },
            { "unsubscribeUrl", unsubscribeUrl }
        };

        await _emailService.SendBulkEmail(
            chapter,
            members,
            EmailType.EventInvite,
            parameters);
    }

    public async Task SendGroupApprovedEmail(Chapter chapter, Member owner)
    {
        var url = _urlProvider.GroupUrl(chapter);

        var subject = "{title} - Your group has been approved 🚀";

        var body = new EmailBodyBuilder()
            .AddParagraph("Your group <strong>{chapter.name}</strong> has been approved and you are ready to go!")
            .AddParagraphLink("url")
            .ToString();

        var to = owner.ToEmailAddressee();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendMemberEmail(
            chapter, 
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendMemberApprovedEmail(Chapter chapter, Member member)
    {
        var url = _urlProvider.GroupUrl(chapter);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        var subject = "{title} - You have been approved by {chapter.name}";
        var body = new EmailBodyBuilder()
            .AddParagraph("Your application to join {chapter.name} has been approved")
            .AddParagraphLink("url")
            .ToString();

        await _emailService.SendMemberEmail(chapter, member.ToEmailAddressee(), subject, body, parameters);
    }

    public async Task<ServiceResult> SendIssueReply(
        Issue issue,
        IssueMessage reply,
        Member member)
    {

    }

    public async Task SendMemberChapterSubscriptionConfirmationEmail(
        Chapter chapter,
        ChapterPaymentSettings chapterPaymentSettings,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc)
    {
        var parameters = new Dictionary<string, string>
        {
            { "subscription.amount", chapterPaymentSettings.Currency.ToAmountString(chapterSubscription.Amount) },
            { "subscription.end", chapter.ToChapterTime(expiresUtc).ToString("d MMMM yyyy") }
        };

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.SubscriptionConfirmation, parameters);
    }

    public async Task SendMemberChapterSubscriptionExpiringEmail(
        Chapter chapter,
        Member member,
        MemberSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate)
    {
        var expiring = expires > DateTime.UtcNow;

        var properties = new Dictionary<string, string>
        {
            { "chapter.name", chapter.Name },
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

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), emailType, properties);
    }

    public async Task SendMemberDeleteEmail(Chapter chapter, Member member, string? reason)
    {
        var subject = "{title} - you have been removed from a group";

        var bodyBuilder = new EmailBodyBuilder()
            .AddParagraph("You have been removed from the {chapter.name} group");

        if (!string.IsNullOrEmpty(reason))
        {
            bodyBuilder
                .AddParagraph("The following reason was given:")
                .AddParagraph("{reason}");
        }

        var body = bodyBuilder.ToString();

        var parameters = new Dictionary<string, string>
        {
            { "reason", reason ?? "" }
        };

        await _emailService.SendEmail(chapter, [member.ToEmailAddressee()], subject, body, parameters);
    }

    public async Task SendMemberLeftChapterEmail(
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

        var subject = "{title} - {member.name} has left {chapter.name}";

        var bodyBuilder = new EmailBodyBuilder()
            .AddParagraph("{member.name} has left {chapter.name}")
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
                { "chapter.name", chapter.Name },
                { "joined", memberChapter?.CreatedUtc.ToFriendlyDateString(chapter.TimeZone) ?? "-" },
                { "reason", reason ?? "" }
            };

        await _emailService.SendEmail(chapter, emailRecipients, subject: subject, body: body, parameters: parameters);
    }

    public async Task SendNewGroupEmail(Chapter chapter, ChapterTexts texts, SiteEmailSettings settings)
    {
        var baseUrl = _urlProvider.BaseUrl();
        var url = $"{baseUrl}/superadmin/groups";

        var parameters = new Dictionary<string, string>
        {
            { "chapter.description", texts.Description ?? "" },
            { "chapter.name", chapter.Name },
            { "url", url }
        };

        var to = new EmailAddressee(settings.ContactEmailAddress, "");

        var subject = "{title} - New group";

        var body = new EmailBodyBuilder()
            .AddParagraph("A group has just been created")
            .AddParagraph("Name: {chapter.name}")
            .AddParagraph("{chapter.description}")
            .AddParagraphLink("url")
            .ToString();

        await _emailService.SendMemberEmail(
            null, 
            to,
            subject,
            body,
            parameters);
    }

    public async Task SendNewIssueEmail(Member member, Issue issue, IssueMessage message, SiteEmailSettings settings)
    {
        var subject = "{title} - New issue";

        var body = new EmailBodyBuilder()
            .AddParagraph("A new issue has been created by {member.name}:")
            .AddParagraph("{issue.title}")
            .AddParagraph("{issue.message}")
            .AddParagraphLink("url")
            .ToString();

        var to = new EmailAddressee(settings.ContactEmailAddress, "");
        await _emailService.SendMemberEmail(null, to, subject, body, new Dictionary<string, string>
        {
            { "issue.title", issue.Title },
            { "issue.title", issue.Title },
            { "member.name", member.FullName },
            { "url", _urlProvider.IssueAdminUrl(issue.Id) }
        });
    }

    public async Task SendNewMemberAdminEmail(
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

        var url = _urlProvider.MemberAdminUrl(chapter, member.Id);

        var parameters = new Dictionary<string, string>
        {
            { "html:member.properties", memberPropertiesBuilder.ToString() },
            { "url", url }
        };

        var to = adminMembers
            .Where(x => x.ReceiveNewMemberEmails)
            .Select(x => x.ToEmailAddressee())
            .ToArray();

        await _emailService.SendEmail(chapter, to, EmailType.NewMemberAdmin, parameters);
    }

    public async Task SendNewMemberEmailsAsync(
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var eventsUrl = _urlProvider.EventsUrl(chapter);

        var parameters = new Dictionary<string, string>
        {
            { "eventsUrl", eventsUrl },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) }
        };

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.NewMember, parameters);

        await SendNewMemberAdminEmail(chapter, adminMembers, member, chapterProperties, memberProperties);
    }

    public async Task SendNewTopicEmail(IReadOnlyCollection<INewTopic> newTopics, SiteEmailSettings settings)
    {
        var url = _urlProvider.TopicApprovalUrl();

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

        var to = new EmailAddressee(settings.ContactEmailAddress, "");

        await _emailService.SendMemberEmail(null, to, subject, body, parameters);
    }

    public async Task SendPasswordResetEmail(Chapter? chapter, Member member, string token)
    {
        string url = _urlProvider.PasswordReset(chapter, token);

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.PasswordReset, parameters);
    }

    public async Task SendSiteMessage(SiteContactMessage message, SiteEmailSettings settings)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "message.from", message.FromAddress },
            { "message.text", message.Message },
            { "url", _urlProvider.MessageAdminUrl(message.Id) }
        };

        var to = new EmailAddressee(settings.ContactEmailAddress, "");

        await _emailService.SendEmail(null, to, EmailType.ContactRequest, parameters);
    }

    public async Task<ServiceResult> SendSiteMessageReply(SiteContactMessage originalMessage, string reply)
    {        
        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, "")
        };

        var subject = "Re: your message to {title}";

        var body = new EmailBodyBuilder()
            .AddText(reply)
            .AddLine()
            .AddParagraph("Your original message:")
            .AddText(originalMessage.Message)
            .ToString();

        return await _emailService.SendEmail(null, to, subject, body);
    }

    public async Task SendSiteSubscriptionExpiredEmail(Member member)
    {
        var url = _urlProvider.MemberSiteSubscriptionUrl();

        var subject = "{title} - Subscription Expired";
        var body = new EmailBodyBuilder()
            .AddParagraph("Your subscription has now expired")
            .AddParagraphLink("url")
            .ToString();

        var parameters = new Dictionary<string, string>
        {
            { "url", url }
        };

        await _emailService.SendMemberEmail(null, member.ToEmailAddressee(), subject, body);
    }

    public async Task SendSiteWelcomeEmail(Member member)
    {
        var url = _urlProvider.GroupsUrl();

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

        await _emailService.SendMemberEmail(null, member.ToEmailAddressee(),
            subject,
            body,
            parameters);
    }

    public async Task<ServiceResult> SendTestEmail(Chapter? chapter, Member to, EmailType type)
    {
        var parameters = new Dictionary<string, string>
        {
            { "member.emailAddress", to.FirstName },
            { "member.firstName", to.FirstName },
            { "member.lastName", to.FirstName }
        };

        return await _emailService.SendEmail(chapter, to.ToEmailAddressee(), type, parameters);
    }

    public async Task SendTopicApprovedEmails(
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

            await _emailService.SendMemberEmail(null, member.ToEmailAddressee(), subject, body, parameters);
        }
    }

    public async Task SendTopicRejectedEmails(
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

            await _emailService.SendMemberEmail(null, member.ToEmailAddressee(), subject, body, parameters);
        }
    }
}
