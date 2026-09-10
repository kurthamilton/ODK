using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Payments;
using ODK.Core.Topics;
using ODK.Core.Venues;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Emails.Parameters;
using ODK.Services.Web;

namespace ODK.Services.Members;

public class MemberEmailService : IMemberEmailService
{
    /* Stands in for a real email while the layout that wraps it is being previewed. Enough of one to show
       where the body sits and that the layout's own styling reaches it. */
    private const string LayoutPreviewBody =
        "<p>EMAIL BODY</p>" +
        "<p>The email being sent appears here, inside the layout.</p>";

    private readonly IEmailService _emailService;
    private readonly IMemberLocaleService _memberLocaleService;
    private readonly ITestEmailParametersFactory _testEmailParametersFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public MemberEmailService(
        IEmailService emailService,
        IUrlProviderFactory urlProviderFactory,
        IUnitOfWork unitOfWork,
        IMemberLocaleService memberLocaleService,
        ITestEmailParametersFactory testEmailParametersFactory)
    {
        _emailService = emailService;
        _memberLocaleService = memberLocaleService;
        _testEmailParametersFactory = testEmailParametersFactory;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<RenderedEmail> RenderTestEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type,
        string subject,
        string body)
    {
        var parameters = await TestEmailParameters(request, chapter, to, type);

        /* The layout template wraps a body rather than being one, so an edited layout is previewed around a
           stand-in body. Every other type is the body, and takes the stored layout. */
        var isLayout = type == EmailType.Layout;

        return await _emailService.RenderEmail(request, new RenderEmailOptions
        {
            BodyHtml = isLayout ? LayoutPreviewBody : body,
            Chapter = chapter,
            Layout = isLayout ? body : null,
            Parameters = parameters,
            /* Only read for the layout, which has no email row to declare an audience. Stated rather than
               left to default, because the default is None and {title} would resolve through it. */
            RecipientType = EmailRecipientType.Members,
            Subject = subject,
            Type = type
        });
    }

    public async Task SendActivationEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string token)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.ActivateAccountUrl(chapter, token);

        var to = member.ToEmailAddressee();

        var parameters = new ActivateAccountParameters
        {
            ActivateAccountUrl = url
        };

        await _emailService.SendEmail(request, chapter, to, EmailType.ActivateAccount, parameters);
    }

    public async Task SendAddressUpdateEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string newEmailAddress,
        string token)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.ConfirmEmailAddressUpdate(chapter, token);

        var to = new EmailAddressee(newEmailAddress, member.FullName);

        var parameters = new EmailAddressUpdateParameters
        {
            Url = url
        };

        await _emailService.SendEmail(request, chapter, to, EmailType.EmailAddressUpdate, parameters);
    }

    public async Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body)
    {
        await _emailService.SendBulkEmail(
            request,
            to,
            subject,
            body,
            EmailRecipientType.Members);
    }

    public async Task SendChapterConversationEmail(
        IChapterServiceRequest request,
        ChapterConversation conversation,
        ChapterConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply)
    {
        var chapter = request.Chapter;

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

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = isToMember
            ? urlProvider.ConversationUrl(chapter, conversation.Id)
            : urlProvider.ConversationAdminUrl(chapter, conversation.Id);

        var addressees = to.Select(x => x.ToEmailAddressee());

        var parameters = new ConversationParameters
        {
            Message = message.Text,
            Subject = ConversationSubject(conversation.Subject, isReply),
            Url = url
        };

        // Same conversation, read by whichever side is being written to - hence the same choice as the url.
        var type = isToMember ? EmailType.ConversationMessage : EmailType.ConversationMessageAdmin;

        await _emailService.SendEmail(request, chapter, addressees, type, parameters);
    }

    public async Task SendSiteConversationEmail(
        IServiceRequest request,
        SiteConversation conversation,
        SiteConversationMessage message,
        IReadOnlyCollection<Member> to,
        bool isReply)
    {
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

        /* No chapter anywhere in this: a site conversation belongs to no group, so the URLs are the
           site-level ones and the email is sent without one. */
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);
        var url = isToMember
            ? urlProvider.SiteConversationUrl(conversation.Id)
            : urlProvider.SiteConversationAdminUrl(conversation.Id);

        var addressees = to.Select(x => x.ToEmailAddressee());

        var parameters = new ConversationParameters
        {
            Message = message.Text,
            Subject = ConversationSubject(conversation.Subject, isReply),
            Url = url
        };

        var type = isToMember
            ? EmailType.SiteConversationMessage
            : EmailType.SiteConversationMessageAdmin;

        await _emailService.SendEmail(request, chapter: null, addressees, type, parameters);
    }

    public async Task SendChapterMessage(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        ChapterContactMessage message)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.MessageAdminUrl(chapter, message.Id);

        var parameters = new ContactRequestParameters
        {
            From = message.FromAddress,
            Text = message.Message,
            Url = url
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
        IChapterServiceRequest request,
        ChapterContactMessage originalMessage,
        string reply)
    {
        var chapter = request.Chapter;

        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, string.Empty)
        };

        var parameters = new ContactRequestReplyParameters
        {
            ReplyHtml = reply,
            Text = originalMessage.Message
        };

        return await _emailService.SendEmail(
            request,
            chapter,
            to,
            EmailType.ContactRequestReply,
            parameters);
    }

    public async Task SendDuplicateMemberEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter);

        var url = urlProvider.LoginUrl(chapter);

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.DuplicateEmail,
            new DuplicateEmailParameters
            {
                LoginUrl = url
            });
    }

    public async Task SendEventCommentEmail(
        IChapterServiceRequest request,
        Event @event,
        EventComment eventComment,
        Member? parentCommentMember)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.EventUrl(chapter, @event.Shortcode);

        var parameters = new EventCommentParameters(@event)
        {
            EventUrl = url,
            Text = eventComment.Text
        };

        await _emailService.SendEventCommentEmail(
            request,
            chapter,
            parentCommentMember,
            eventComment,
            parameters);
    }

    public async Task SendEventInvites(
        IChapterServiceRequest request,
        Event @event,
        Venue venue,
        IEnumerable<Member> members)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var eventUrl = urlProvider.EventUrl(chapter, @event.Shortcode);
        var rsvpUrl = @event.Ticketed
            ? urlProvider.EventUrl(chapter, @event.Shortcode)
            : urlProvider.EventRsvpUrl(chapter, @event.Shortcode);
        var unsubscribeUrl = urlProvider.EmailPreferences(chapter);

        // Each recipient's date is formatted in their own locale (default fallback), so group recipients by
        // culture and send one bulk email per group.
        var memberList = members.ToArray();
        var cultures = await _memberLocaleService.GetCultures(memberList.Select(x => x.Id).ToArray());

        foreach (var group in memberList.GroupBy(x => cultures[x.Id]))
        {
            var parameters = new EventInviteParameters(chapter, @event, venue, group.Key)
            {
                RsvpUrl = rsvpUrl,
                UnsubscribeUrl = unsubscribeUrl,
                Url = eventUrl
            };

            await _emailService.SendBulkEmail(request, group, EmailType.EventInvite, parameters);
        }
    }

    public async Task SendEventWaitlistPromotionNotification(
        IChapterServiceRequest request,
        Event @event,
        IEnumerable<Member> members)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.EventUrl(chapter, @event.Shortcode);

        // Each recipient's date is formatted in their own locale (default fallback), so group recipients by
        // culture and send one email per group.
        var memberList = members.ToArray();
        var cultures = await _memberLocaleService.GetCultures(memberList.Select(x => x.Id).ToArray());

        foreach (var group in memberList.GroupBy(x => cultures[x.Id]))
        {
            var parameters = new EventWaitlistPromotionParameters(chapter, @event, group.Key)
            {
                Url = url
            };

            var to = group.Select(x => x.ToEmailAddressee()).ToArray();

            await _emailService.SendEmail(
                request, chapter, to, EmailType.EventWaitlistPromotion, parameters);
        }
    }

    public async Task SendGroupApprovedEmail(
        IChapterServiceRequest request,
        Member owner)
    {
        var chapter = request.Chapter;

        /* No parameters: the group is the one the email is sent as, so its name and address are already
           among the core values every email gets. */
        await _emailService.SendEmail(
            request,
            chapter,
            owner.ToEmailAddressee(),
            EmailType.GroupApproved,
            parameters: null);
    }

    public async Task SendMemberApprovedEmail(
        IChapterServiceRequest request,
        Member member)
    {
        var chapter = request.Chapter;

        // No parameters, for the same reason as SendGroupApprovedEmail.
        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.MemberApproved,
            parameters: null);
    }

    public async Task SendMemberChapterSubscriptionConfirmationEmail(
        IChapterServiceRequest request,
        ChapterSubscription chapterSubscription,
        Member member,
        DateTime expiresUtc)
    {
        var chapter = request.Chapter;

        var currency = chapterSubscription.Currency;
        var culture = await _memberLocaleService.GetCulture(member.Id);

        var parameters = new SubscriptionConfirmationParameters(currency, member, culture)
        {
            Amount = chapterSubscription.Amount,
            ExpiresUtc = expiresUtc
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.SubscriptionConfirmation,
            parameters);
    }

    public async Task SendMemberChapterSubscriptionExpiringEmail(
        IChapterServiceRequest request,
        Member member,
        MemberChapterSubscription memberSubscription,
        DateTime expires,
        DateTime disabledDate)
    {
        var chapter = request.Chapter;

        var expiring = expires > DateTime.UtcNow;
        var culture = await _memberLocaleService.GetCulture(member.Id);

        var parameters = new SubscriptionExpiryParameters(member, culture)
        {
            DisabledUtc = disabledDate,
            ExpiresUtc = expires
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
            parameters);
    }

    public async Task SendMemberDeleteEmail(
        IChapterServiceRequest request,
        Member member,
        string? reason)
    {
        var chapter = request.Chapter;

        var parameters = new MemberRemovedParameters
        {
            Reason = reason
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.MemberRemoved,
            parameters);
    }

    public async Task SendMemberImportInviteEmail(
        IChapterServiceRequest request,
        Member member,
        string inviteToken)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);

        var parameters = new MemberImportInviteParameters
        {
            RefuseUrl = urlProvider.RefuseInviteUrl(chapter, inviteToken),
            Url = urlProvider.AcceptInviteUrl(chapter, inviteToken)
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.MemberImportInvite,
            parameters);
    }

    public async Task SendMemberLeftChapterEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        string? reason)
    {
        var chapter = request.Chapter;

        var recipients = adminMembers
            .Where(x => x.MemberId != member.Id && x.ReceiveNewMemberEmails)
            .ToArray();

        if (recipients.Length == 0)
        {
            return;
        }

        var memberChapter = member.MemberChapter(chapter.Id);

        // Each recipient admin's "joined" date is formatted in their own locale (default fallback), so
        // group recipients by culture and send one email per group.
        var cultures = await _memberLocaleService.GetCultures(recipients.Select(x => x.MemberId).ToArray());

        foreach (var group in recipients.GroupBy(x => cultures[x.MemberId]))
        {
            var parameters = new MemberLeftParameters(member, group.Key, chapter.TimeZone)
            {
                JoinedUtc = memberChapter?.CreatedUtc,
                Reason = reason
            };

            var to = group.Select(x => x.ToEmailAddressee()).ToArray();

            await _emailService.SendEmail(
                request, chapter, to, EmailType.MemberLeftAdmin, parameters);
        }
    }

    public async Task SendNewGroupEmail(
        IServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> siteAdmins)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);

        var parameters = new NewGroupAdminParameters(chapter)
        {
            GroupsUrl = urlProvider.SiteAdminGroups()
        };

        var to = siteAdmins
            .Select(x => x.ToEmailAddressee())
            .ToArray();

        /* Sent as the site and not as the new group: it goes to site admins, so it reads as the platform's
           own and takes the site's template. The group it is about reaches it as a parameter. */
        await _emailService.SendEmail(
            request,
            chapter: null,
            to,
            EmailType.NewGroupAdmin,
            parameters);
    }

    public async Task SendNewMemberAdminEmail(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var chapter = request.Chapter;

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

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.MemberAdminUrl(chapter, member.Id);

        var parameters = new NewMemberAdminParameters
        {
            AdminUrl = url,
            PropertiesHtml = memberPropertiesBuilder.ToString()
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

    public async Task SendNewMemberEmails(
        IChapterServiceRequest request,
        IReadOnlyCollection<ChapterAdminMember> adminMembers,
        Member member,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var chapter = request.Chapter;

        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var eventsUrl = urlProvider.EventsUrl(chapter);

        var parameters = new NewMemberParameters
        {
            EventsUrl = eventsUrl,
            FirstName = HttpUtility.HtmlEncode(member.FirstName)
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.NewMember,
            parameters);

        await SendNewMemberAdminEmail(
            request,
            adminMembers,
            member,
            chapterProperties,
            memberProperties);
    }

    public async Task SendNewTopicEmail(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IEnumerable<Member> siteAdmins)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);

        var parameters = new NewTopicAdminParameters(newTopics)
        {
            Url = urlProvider.TopicApprovalUrl()
        };

        var to = siteAdmins.Select(x => x.ToEmailAddressee()).ToArray();

        await _emailService.SendEmail(
            request,
            chapter: null,
            to,
            EmailType.NewTopicAdmin,
            parameters);
    }

    public async Task SendPasswordResetEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member member,
        string token)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter);
        var url = urlProvider.PasswordReset(chapter, token);

        var parameters = new PasswordResetParameters
        {
            Url = url
        };

        await _emailService.SendEmail(
            request,
            chapter,
            member.ToEmailAddressee(),
            EmailType.PasswordReset,
            parameters);
    }

    public async Task SendPaymentNotification(
        IServiceRequest request,
        Member member,
        Chapter? chapter,
        Payment payment,
        Currency currency)
    {
        /* Sent as the group where the payment was made to one - a membership or a ticket is the group's own
           transaction with its member, so the receipt carries the group's title, theme, layout and any wording
           it has overridden. A payment to the site has no group, and comes from the site. */
        await _emailService.SendEmail(
            request,
            chapter,
            [member.ToEmailAddressee()],
            EmailType.PaymentNotification,
            new PaymentNotificationParameters(currency)
            {
                Amount = payment.Amount,
                Reference = payment.Reference
            });
    }

    public async Task SendSiteMessage(
        IServiceRequest request,
        SiteContactMessage message,
        IEnumerable<Member> siteAdmins)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);
        var url = urlProvider.MessageSiteAdminUrl(message.Id);

        var parameters = new ContactRequestParameters
        {
            From = message.FromAddress,
            Text = message.Message,
            Url = url
        };

        var to = siteAdmins.Select(x => x.ToEmailAddressee());

        await _emailService.SendEmail(
            request,
            null,
            to,
            EmailType.ContactRequest,
            parameters);
    }

    public async Task<ServiceResult> SendSiteMessageReply(
        IServiceRequest request,
        SiteContactMessage originalMessage,
        string reply)
    {
        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, string.Empty)
        };

        var parameters = new ContactRequestReplyParameters
        {
            ReplyHtml = reply,
            Text = originalMessage.Message
        };

        return await _emailService.SendEmail(
            request,
            chapter: null,
            to,
            EmailType.SiteContactRequestReply,
            parameters);
    }

    public async Task SendSiteSubscriptionExpiredEmail(
        IServiceRequest request,
        Member member)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);

        var parameters = new SiteSubscriptionExpiredParameters
        {
            SubscriptionUrl = urlProvider.MemberSiteSubscriptionUrl()
        };

        await _emailService.SendEmail(
            request,
            chapter: null,
            member.ToEmailAddressee(),
            EmailType.SiteSubscriptionExpired,
            parameters);
    }

    public async Task SendSiteWelcomeEmail(
        IServiceRequest request,
        Member member)
    {
        var urlProvider = _urlProviderFactory.Create(request, chapter: null);

        var parameters = new SiteWelcomeParameters(member)
        {
            GroupsUrl = urlProvider.GroupsUrl()
        };

        await _emailService.SendEmail(
            request,
            chapter: null,
            member.ToEmailAddressee(),
            EmailType.SiteWelcome,
            parameters);
    }

    public async Task<ServiceResult> SendTestEmail(
        IServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type)
    {
        var parameters = await TestEmailParameters(request, chapter, to, type);

        return await _emailService.SendEmail(
            request,
            chapter,
            to.ToEmailAddressee(),
            type,
            parameters);
    }

    public Task SendTopicApprovedEmails(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members)
        => SendTopicDecisionEmails(request, newTopics, members, EmailType.TopicsApproved);

    public Task SendTopicRejectedEmails(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members)
        => SendTopicDecisionEmails(request, newTopics, members, EmailType.TopicsRejected);


    /* The marker goes on the conversation's subject rather than on the whole line, because a stored
       subject template cannot vary by whether a message is a reply. The default templates open with the
       parameter, so the email reads as it always has. */
    private static string ConversationSubject(string subject, bool isReply)
        => isReply ? $"Re: {subject}" : subject;

    /* Ordered by name rather than by when they joined: this only stands in for a group the caller did not
       name, so any of theirs makes the email concrete and the ordering only has to be stable enough that
       the same one turns up each time. Null when they belong to no group, which leaves the group
       parameters to fall back to the platform's own details as they did before. */
    private async Task<Chapter?> GetFirstChapter(IServiceRequest request, Member member)
    {
        var chapters = await _unitOfWork.ChapterRepository
            .GetByMemberId(request.Platform, member.Id)
            .Run();

        return chapters
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    /* One member's topics, one email, whichever way they were decided. Members with none of their own are
       skipped rather than sent an empty table. */
    private async Task SendTopicDecisionEmails(
        IServiceRequest request,
        IReadOnlyCollection<INewTopic> newTopics,
        IReadOnlyCollection<Member> members,
        EmailType type)
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

            await _emailService.SendEmail(
                request,
                chapter: null,
                member.ToEmailAddressee(),
                type,
                new MemberTopicsParameters(memberTopics));
        }
    }

    private async Task<IEmailParameters> TestEmailParameters(
        IServiceRequest request,
        Chapter? chapter,
        Member to,
        EmailType type)
    {
        /* The group the email describes, which is not the group it is sent as. chapter is passed on
           untouched so the template lookup is unaffected - a site admin testing the site's copy of a
           template must not be sent a stand-in group's override of it. */
        var describedChapter = chapter ?? await GetFirstChapter(request, to);

        var culture = await _memberLocaleService.GetCulture(to.Id);

        return _testEmailParametersFactory.Create(request, type, to, culture, describedChapter);
    }
}