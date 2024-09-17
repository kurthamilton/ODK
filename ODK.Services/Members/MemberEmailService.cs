using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Emails;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    private readonly IEmailService _emailService;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public MemberEmailService(
        IEmailService emailService, 
        IUrlProvider urlProvider,
        IHttpRequestProvider httpRequestProvider,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _httpRequestProvider = httpRequestProvider;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }

    public async Task SendActivationEmail(Chapter? chapter, Member member, string token)
    {        
        var url = _urlProvider.ActivateAccountUrl(chapter, token);

        var to = member.ToEmailAddressee();

        await _emailService.SendEmail(chapter, to, EmailType.ActivateAccount, new Dictionary<string, string>
        {
            { "url", url }
        });
    }

    public async Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token)
    {
        var url = _urlProvider.ConfirmEmailAddressUpdate(chapter, token);

        var to = new EmailAddressee(newEmailAddress, member.FullName);

        await _emailService.SendEmail(chapter, to, EmailType.EmailAddressUpdate,
            new Dictionary<string, string>
            {
                { "url", url }
            });
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

        await _emailService.SendEmail(chapter, addressees, subject, body, new Dictionary<string, string>
        {
            { "conversation.subject", conversation.Subject },
            { "conversation.message", message.Text },
            { "url", url }
        });
    }

    public async Task SendChapterMessage(
        Chapter chapter, 
        IReadOnlyCollection<ChapterAdminMember> adminMembers, 
        ChapterContactMessage message)
    {
        var url = _urlProvider.MessageAdminUrl(chapter, message.Id);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"message.from", message.FromAddress},
            {"message.text", message.Message},
            {"url", url}
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
        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, "")
        };

        var body = new EmailBodyBuilder()
            .AddText(reply)
            .AddLine()
            .AddParagraph("Your original message:")
            .AddText(originalMessage.Message)
            .ToString();

        return await _emailService.SendEmail(chapter, to, "Re: your message to {title}", body);
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
        var parameters = new Dictionary<string, string>
        {
            { "comment.text", eventComment.Text },
            { "event.id", @event.Id.ToString() }
        };

        var url = _urlProvider.EventUrl(chapter, @event.Id);
        parameters.Add("event.url", url);

        await _emailService.SendEventCommentEmail(chapter, parentCommentMember, eventComment, parameters);
    }

    public async Task SendEventInvites(
        Chapter chapter,
        Event @event,
        Venue venue,
        IEnumerable<Member> members)
    {
        var time = @event.ToLocalTimeString(chapter.TimeZone);

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"event.date", @event.Date.ToString("dddd dd MMMM, yyyy")},
            {"event.id", @event.Id.ToString()},
            {"event.location", venue.Name},
            {"event.name", @event.Name},
            {"event.time", time}
        };

        var eventUrl = _urlProvider.EventUrl(chapter, @event.Id);
        var rsvpUrl = _urlProvider.EventRsvpUrl(chapter, @event.Id);
        var unsubscribeUrl = _urlProvider.EmailPreferences(chapter);

        parameters.Add("event.rsvpurl", rsvpUrl);
        parameters.Add("event.url", eventUrl);
        parameters.Add("unsubscribeUrl", unsubscribeUrl);

        await _emailService.SendBulkEmail(
            chapter,
            members,
            EmailType.EventInvite,
            parameters);
    }

    public async Task SendGroupApprovedEmail(Chapter chapter, Member owner)
    {
        var url = _urlProvider.GroupUrl(chapter);

        var subject = "{title} - Your group has been approved";

        var body = new EmailBodyBuilder()
            .AddParagraph("Thank you for starting the group '{chapter.name}'. It has now been approved and you are ready to go!")
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
        var subject = "{title} - you have been removed";

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
                { "joined", memberChapter.CreatedUtc.ToFriendlyDateString(chapter.TimeZone) ?? "-" },
                { "reason", reason ?? "" }
            };

        await _emailService.SendEmail(chapter, emailRecipients, subject: subject, body: body, parameters: parameters);
    }

    public async Task SendNewGroupEmail(Chapter chapter, ChapterTexts texts, SiteEmailSettings settings)
    {
        var baseUrl = UrlUtils.BaseUrl(_httpRequestProvider.RequestUrl);
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

    public async Task SendNewMemberAdminEmail(
        Chapter chapter,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var parameters = new Dictionary<string, string>
        {
            { "member.firstName", member.FirstName },
            { "member.lastName", member.LastName }
        };

        foreach (var chapterProperty in chapterProperties)
        {
            string? value = memberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id)?.Value;
            parameters.Add($"member.properties.{chapterProperty.Name}", value ?? "");
        }

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

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.NewMember, new Dictionary<string, string>
        {
            { "eventsUrl", eventsUrl },
            { "member.firstName", HttpUtility.HtmlEncode(member.FirstName) }
        });

        await SendNewMemberAdminEmail(chapter, adminMembers, member, chapterProperties, memberProperties);
    }

    public async Task SendPasswordResetEmail(Chapter? chapter, Member member, string token)
    {
        string url = _urlProvider.PasswordReset(chapter, token);

        await _emailService.SendEmail(chapter, member.ToEmailAddressee(), EmailType.PasswordReset, new Dictionary<string, string>
        {
            { "url", url }
        });
    }

    public async Task SendSiteMessage(SiteContactMessage message, SiteEmailSettings settings)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"message.from", message.FromAddress},
            {"message.text", message.Message},
            {"url", _urlProvider.MessageAdminUrl(message.Id)}
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
}
