using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Emails.Parameters;
using ODK.Services.Members;
using ODK.Services.Tests.Helpers;
using ODK.Services.Web;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberEmailServiceTests
{
    [Test]
    public static async Task RenderTestEmail_OrdinaryType_RendersTheSuppliedWordingAsTheBody()
    {
        // Arrange
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.RenderTestEmail(
            request, chapter, member, EmailType.NewMember, "A subject", "<p>A body</p>");

        // Assert - no layout of its own, so the stored one wraps it.
        emailService.Verify(
            x => x.RenderEmail(
                request,
                It.Is<RenderEmailOptions>(x =>
                    x.BodyHtml == "<p>A body</p>" &&
                    x.Layout == null &&
                    x.Subject == "A subject" &&
                    x.Type == EmailType.NewMember)),
            Times.Once);
    }

    [Test]
    public static async Task RenderTestEmail_TheLayout_RendersTheSuppliedWordingAroundAStandInBody()
    {
        /* Arrange - the layout wraps a body rather than being one. Rendered as the body it would show its own
           markup as the email's content, and nothing of what it wraps. */
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        var layout = "<html><body>{body}</body></html>";

        // Act
        await service.RenderTestEmail(request, chapter, member, EmailType.Layout, string.Empty, layout);

        // Assert
        emailService.Verify(
            x => x.RenderEmail(
                request,
                It.Is<RenderEmailOptions>(x =>
                    x.Layout == layout &&
                    x.BodyHtml != layout &&
                    x.BodyHtml.Length > 0)),
            Times.Once);
    }

    [Test]
    public static async Task SendChapterConversationEmail_AReply_MarksTheConversationSubject()
    {
        /* Arrange - the stored subject template cannot vary by whether a message is a reply, so the marker
           arrives on the parameter the template opens with. */
        var chapter = CreateChapter();
        var member = CreateMember();
        var conversation = CreateChapterConversation(chapter, member);

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act - written by the conversation's own member, so it goes to the group's admins.
        await service.SendChapterConversationEmail(
            request,
            conversation,
            CreateChapterConversationMessage(conversation, member.Id),
            [member],
            isReply: true);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.ConversationMessageAdmin,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["conversation.subject"] == "Re: Test conversation")),
            Times.Once);
    }

    [Test]
    public static async Task SendChapterConversationEmail_ToTheMember_SendsTheMemberTemplate()
    {
        // Arrange - the audience decides the template, because a row states one recipient type.
        var chapter = CreateChapter();
        var member = CreateMember();
        var admin = CreateMember();
        var conversation = CreateChapterConversation(chapter, member);

        using var context = new MockOdkContext();

        var emailService = CreateEmailService();
        var service = CreateService(
            emailService, new Mock<IUrlProvider>(), MockUnitOfWorkFactory.Create(context));

        var request = CreateChapterServiceRequest(chapter);

        // Act - written by someone other than the conversation's member, so the member is the recipient.
        await service.SendChapterConversationEmail(
            request,
            conversation,
            CreateChapterConversationMessage(conversation, admin.Id),
            [member],
            isReply: false);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.ConversationMessage,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["conversation.subject"] == "Test conversation")),
            Times.Once);
    }

    [Test]
    public static async Task SendChapterMessageReply_SendsTheReplyAsHtmlAndTheOriginalAsText()
    {
        /* Arrange - the reply is editor output and keeps its markup; the original message is what someone
           typed into a contact form, so it is encoded like any other value. */
        var chapter = CreateChapter();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        var originalMessage = new ChapterContactMessage
        {
            ChapterId = chapter.Id,
            FromAddress = "asker@example.com",
            Id = Guid.NewGuid(),
            Message = "The original question"
        };

        // Act
        await service.SendChapterMessageReply(request, originalMessage, "<p>An answer</p>");

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<IEnumerable<EmailAddressee>>(x => x.Single().Address == "asker@example.com"),
                EmailType.ContactRequestReply,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["html:message.reply"] == "<p>An answer</p>" &&
                    x.ToDictionary()["message.text"] == "The original question")),
            Times.Once);
    }

    [Test]
    public static async Task SendEventWaitlistPromotionNotification_DatesTheEventInTheGroupsTimeZone()
    {
        /* Arrange - an event's wall-clock time is the venue's, so the date follows the group's zone rather
           than UTC. Late enough in the day that the two fall on different dates. */
        var chapter = CreateChapter();
        chapter.TimeZoneId = "Tokyo Standard Time";

        var member = CreateMember();

        var @event = new Event
        {
            ChapterId = chapter.Id,
            DateUtc = new DateTime(2026, 5, 1, 22, 0, 0, DateTimeKind.Utc),
            Id = Guid.NewGuid(),
            Name = "Test event",
            PublishedUtc = DateTime.UtcNow,
            Shortcode = "abc123"
        };

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act
        await service.SendEventWaitlistPromotionNotification(request, @event, [member]);

        // Assert - 1 May in UTC, 2 May in Tokyo.
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.EventWaitlistPromotion,
                It.Is<IEmailParameters>(x => x.ToDictionary()["event.date"].Contains("02 May"))),
            Times.Once);
    }

    [Test]
    public static async Task SendGroupApprovedEmail_SendsAsTheGroupWithNoParametersOfItsOwn()
    {
        // Arrange - the group is the one it is sent as, so the core values already name it.
        var chapter = CreateChapter();
        var owner = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act
        await service.SendGroupApprovedEmail(request, owner);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == owner.EmailAddress),
                EmailType.GroupApproved,
                null),
            Times.Once);
    }

    [Test]
    public static async Task SendMemberApprovedEmail_SendsAsTheGroupWithNoParametersOfItsOwn()
    {
        // Arrange
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act
        await service.SendMemberApprovedEmail(request, member);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.MemberApproved,
                null),
            Times.Once);
    }

    [Test]
    public static async Task SendMemberImportInviteEmail_SendsInviteTemplateWithTheAcceptUrl()
    {
        /* Arrange - the page where an invite is accepted, which the template names group.urls.join. The
           parameter is older than that page being platform-specific, so the two names differ on purpose. */
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider
            .Setup(x => x.AcceptInviteUrl(chapter, "invite-token"))
            .Returns("https://test.local/group/accept-invite");

        var service = CreateService(emailService, urlProvider);

        var request = new ChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMemberOrDefault = null,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendMemberImportInviteEmail(request, member, "invite-token");

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.MemberImportInvite,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["group.urls.join"] == "https://test.local/group/accept-invite")),
            Times.Once);
    }

    [TestCase("They moved away", "They moved away")]
    [TestCase(null, "-")]
    public static async Task SendMemberLeftChapterEmail_CoalescesAMissingReason(
        string? reason, string expected)
    {
        /* Arrange - the wording used to branch on whether a reason was given, which a stored template
           cannot. The line is always there and the value stands in for itself when there is none. */
        var chapter = CreateChapter();
        var member = CreateMember();
        var admin = CreateChapterAdminMember(chapter);

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act
        await service.SendMemberLeftChapterEmail(request, [admin], member, reason);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.MemberLeftAdmin,
                It.Is<IEmailParameters>(x => x.ToDictionary()["member.leftReason"] == expected)),
            Times.Once);
    }

    [TestCase("They were rude to people", "They were rude to people")]
    [TestCase(null, "-")]
    public static async Task SendMemberDeleteEmail_CoalescesAMissingReason(
        string? reason, string expected)
    {
        /* Arrange - the reason paragraphs used to be added only when there was a reason, which a stored
           template cannot do. The line is always there, and stands in for itself when there is none. */
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateChapterServiceRequest(chapter);

        // Act
        await service.SendMemberDeleteEmail(request, member, reason);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.MemberRemoved,
                It.Is<IEmailParameters>(x => x.ToDictionary()["member.removedReason"] == expected)),
            Times.Once);
    }

    [Test]
    public static async Task SendNewGroupEmail_SendsAsTheSiteAndNamesTheNewGroup()
    {
        /* Arrange - it goes to site admins, so it is addressed from the platform and takes the site's
           template. The group it is about cannot be group.name, which is what an email is sent as. */
        var chapter = CreateChapter();
        var siteAdmin = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendNewGroupEmail(request, chapter, [siteAdmin]);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.NewGroupAdmin,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["newGroup.name"] == chapter.FullName &&
                    !x.ToDictionary().ContainsKey("group.name"))),
            Times.Once);
    }

    [Test]
    public static async Task SendNewTopicEmail_SuppliesTheTopicsAsOneTable()
    {
        /* Arrange - a row per topic used to be a pair of parameters named with an index, which the
           placeholder pattern never admitted. The table is one pre-encoded value. */
        var siteAdmin = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendNewTopicEmail(request, CreateTopics(siteAdmin.Id), [siteAdmin]);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.NewTopicAdmin,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["html:topics"].Contains("Test topic") &&
                    x.ToDictionary()["html:topics"].Contains("Test topic group"))),
            Times.Once);
    }

    [Test]
    public static async Task SendPaymentNotification_ForAGroupsPayment_SendsAsTheGroupWithThePaymentValues()
    {
        /* Arrange - the wording used to be interpolated here, so an admin could not change it. It is a stored
           template now, and sent as the group: a membership payment is the group's transaction with its member,
           so the receipt takes the group's title, theme and layout rather than the site's. */
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendPaymentNotification(
            request, member, chapter, CreatePayment(), CreateCurrency());

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.Is<IEnumerable<EmailAddressee>>(to => to.Any(x => x.Address == member.EmailAddress)),
                EmailType.PaymentNotification,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["payment.amount"] == "£12.34" &&
                    x.ToDictionary()["payment.reference"] == "REF123")),
            Times.Once);
    }

    [Test]
    public static async Task SendPaymentNotification_ForASitePayment_SendsWithNoGroup()
    {
        // Arrange - a payment to the site belongs to no group, so there is none to send as.
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendPaymentNotification(
            request, member, chapter: null, CreatePayment(), CreateCurrency());

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.PaymentNotification,
                It.IsAny<IEmailParameters>()),
            Times.Once);
    }

    [Test]
    public static async Task SendSiteConversationEmail_SendsTheSiteTemplateWithNoGroup()
    {
        // Arrange - a site conversation belongs to no group, so it is sent as the site.
        var member = CreateMember();
        var admin = CreateMember();

        var conversation = new SiteConversation
        {
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Subject = "Test conversation"
        };

        using var context = new MockOdkContext();

        var emailService = CreateEmailService();
        var service = CreateService(
            emailService, new Mock<IUrlProvider>(), MockUnitOfWorkFactory.Create(context));

        var request = CreateServiceRequest();

        var message = new SiteConversationMessage
        {
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = admin.Id,
            SiteConversationId = conversation.Id,
            Text = "Test message"
        };

        // Act
        await service.SendSiteConversationEmail(request, conversation, message, [member], isReply: false);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<IEnumerable<EmailAddressee>>(),
                EmailType.SiteConversationMessage,
                It.IsAny<IEmailParameters>()),
            Times.Once);
    }

    [Test]
    public static async Task SendSiteMessageReply_SendsTheSiteTemplateWithNoGroup()
    {
        // Arrange - the site's own reply, which links nowhere a group would.
        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        var originalMessage = new SiteContactMessage
        {
            FromAddress = "asker@example.com",
            Id = Guid.NewGuid(),
            Message = "The original question"
        };

        // Act
        await service.SendSiteMessageReply(request, originalMessage, "<p>An answer</p>");

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<IEnumerable<EmailAddressee>>(x => x.Single().Address == "asker@example.com"),
                EmailType.SiteContactRequestReply,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["html:message.reply"] == "<p>An answer</p>" &&
                    x.ToDictionary()["message.text"] == "The original question")),
            Times.Once);
    }

    [Test]
    public static async Task SendSiteSubscriptionExpiredEmail_SendsWithTheSubscriptionUrl()
    {
        // Arrange
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider
            .Setup(x => x.MemberSiteSubscriptionUrl())
            .Returns("https://test.local/account/subscription");

        var service = CreateService(emailService, urlProvider);

        var request = CreateServiceRequest();

        // Act
        await service.SendSiteSubscriptionExpiredEmail(request, member);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.SiteSubscriptionExpired,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["account.urls.siteSubscription"] ==
                        "https://test.local/account/subscription")),
            Times.Once);
    }

    [Test]
    public static async Task SendSiteWelcomeEmail_SendsWithTheMembersFirstNameAndTheirGroups()
    {
        // Arrange
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.GroupsUrl()).Returns("https://test.local/my/groups");

        var service = CreateService(emailService, urlProvider);

        var request = CreateServiceRequest();

        // Act
        await service.SendSiteWelcomeEmail(request, member);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.SiteWelcome,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["member.firstName"] == member.FirstName &&
                    x.ToDictionary()["account.urls.groups"] == "https://test.local/my/groups")),
            Times.Once);
    }

    [Test]
    public static async Task SendTestEmail_FillsMemberAndGroupFromTheCurrentMemberAndGroup()
    {
        // Arrange - the point of the test send: an admin checking a template sees it against real values.
        var chapter = CreateChapter();
        var member = CreateMember();

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.EventsUrl(chapter)).Returns("https://test.local/test-group/events");
        urlProvider.Setup(x => x.GroupUrl(chapter)).Returns("https://test.local/test-group");

        var service = CreateService(emailService, urlProvider);

        var request = new MemberChapterServiceRequest
        {
            Chapter = chapter,
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendTestEmail(request, chapter, member, EmailType.NewMember);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                chapter,
                It.IsAny<EmailAddressee>(),
                EmailType.NewMember,
                It.Is<IEmailParameters>(x =>
                    x.ToDictionary()["member.firstName"] == "Test" &&
                    x.ToDictionary()["group.name"] == "Test group" &&
                    x.ToDictionary()["group.urls.events"] == "https://test.local/test-group/events")),
            Times.Once);
    }

    [Test]
    public static async Task SendTestEmail_WithNoCurrentGroup_DescribesTheMembersFirstGroup()
    {
        // Arrange - a site admin testing the site's copy of a template has no current group, so one of
        // theirs stands in rather than the group parameters falling back to the platform's own details.
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        context.CreateChapter(name: "Beta group", members: [member]);
        context.CreateChapter(name: "Alpha group", members: [member]);

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.GroupUrl(It.IsAny<Chapter>())).Returns("https://test.local/alpha-group");

        var service = CreateService(emailService, urlProvider, MockUnitOfWorkFactory.Create(context));

        var request = new MemberServiceRequest
        {
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendTestEmail(request, null, member, EmailType.NewMember);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<EmailAddressee>(),
                EmailType.NewMember,
                It.Is<IEmailParameters>(x => x.ToDictionary()["group.name"] == "Alpha group")),
            Times.Once);
    }

    [Test]
    public static async Task SendTestEmail_WithNoCurrentGroup_StillSendsWithoutAChapter()
    {
        // Arrange - the stand-in group reaches the email as parameters only. Sending as that group would
        // make EmailService look up its override of the template instead of the one being tested.
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        context.CreateChapter(name: "Alpha group", members: [member]);

        var emailService = CreateEmailService();
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.GroupUrl(It.IsAny<Chapter>())).Returns("https://test.local/alpha-group");

        var service = CreateService(emailService, urlProvider, MockUnitOfWorkFactory.Create(context));

        var request = new MemberServiceRequest
        {
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendTestEmail(request, null, member, EmailType.NewMember);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                It.IsAny<IServiceRequest>(),
                null,
                It.IsAny<EmailAddressee>(),
                It.IsAny<EmailType>(),
                It.IsAny<IEmailParameters>()),
            Times.Once);
    }

    [Test]
    public static async Task SendTestEmail_WithNoGroupAtAll_LeavesGroupSpecificTokensUnset()
    {
        // Arrange - the stand-in values an event invite is built from come from a group, so a member who
        // belongs to none leaves them out rather than inventing them. The tokens stay visible in the test
        // email, which is the honest answer for a template that cannot be filled in from who is asking.
        using var context = new MockOdkContext();
        var member = context.CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>(), MockUnitOfWorkFactory.Create(context));

        var request = new MemberServiceRequest
        {
            CurrentMember = member,
            CurrentMemberOrDefault = member,
            Environment = EnvironmentType.Dev,
            HttpRequestContext = CreateHttpRequestContext(),
            Platform = PlatformType.Default
        };

        // Act
        await service.SendTestEmail(request, null, member, EmailType.EventInvite);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.IsAny<EmailAddressee>(),
                EmailType.EventInvite,
                It.Is<IEmailParameters>(x =>
                    !x.ToDictionary().ContainsKey("event.name") &&
                    !x.ToDictionary().ContainsKey("group.name"))),
            Times.Once);
    }

    [Test]
    public static async Task SendTopicApprovedEmails_SendsEachMemberOnlyTheirOwnTopics()
    {
        /* Arrange - one email per member, carrying the topics that member suggested. A member with none of
           their own is skipped rather than sent an empty table. */
        var member = CreateMember();
        var otherMember = CreateMember("other@example.com");

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendTopicApprovedEmails(
            request, CreateTopics(member.Id), [member, otherMember]);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.TopicsApproved,
                It.Is<IEmailParameters>(x => x.ToDictionary()["html:topics"].Contains("Test topic"))),
            Times.Once);

        emailService.Verify(
            x => x.SendEmail(
                request,
                It.IsAny<Chapter?>(),
                It.Is<EmailAddressee>(x => x.Address == otherMember.EmailAddress),
                It.IsAny<EmailType>(),
                It.IsAny<IEmailParameters>()),
            Times.Never);
    }

    [Test]
    public static async Task SendTopicRejectedEmails_SendsTheRejectedTemplate()
    {
        // Arrange - the same values as approval; only the template differs.
        var member = CreateMember();

        var emailService = CreateEmailService();
        var service = CreateService(emailService, new Mock<IUrlProvider>());

        var request = CreateServiceRequest();

        // Act
        await service.SendTopicRejectedEmails(request, CreateTopics(member.Id), [member]);

        // Assert
        emailService.Verify(
            x => x.SendEmail(
                request,
                null,
                It.Is<EmailAddressee>(x => x.Address == member.EmailAddress),
                EmailType.TopicsRejected,
                It.IsAny<IEmailParameters>()),
            Times.Once);
    }

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        Slug = "test-group"
    };

    private static ChapterAdminMember CreateChapterAdminMember(Chapter chapter)
    {
        var member = CreateMember();

        return new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            Member = member,
            MemberId = member.Id,
            ReceiveNewMemberEmails = true
        };
    }

    private static ChapterConversation CreateChapterConversation(Chapter chapter, Member member) => new()
    {
        ChapterId = chapter.Id,
        CreatedUtc = DateTime.UtcNow,
        Id = Guid.NewGuid(),
        MemberId = member.Id,
        Subject = "Test conversation"
    };

    private static ChapterConversationMessage CreateChapterConversationMessage(
        ChapterConversation conversation, Guid fromMemberId) => new()
    {
        ChapterConversationId = conversation.Id,
        CreatedUtc = DateTime.UtcNow,
        Id = Guid.NewGuid(),
        MemberId = fromMemberId,
        Text = "Test message"
    };

    private static ChapterServiceRequest CreateChapterServiceRequest(Chapter chapter) => new()
    {
        Chapter = chapter,
        CurrentMemberOrDefault = null,
        Environment = EnvironmentType.Dev,
        HttpRequestContext = CreateHttpRequestContext(),
        Platform = PlatformType.Default
    };

    private static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>();

        mock
            .Setup(x => x.SendEmail(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Chapter?>(),
                It.IsAny<EmailAddressee>(),
                It.IsAny<EmailType>(),
                It.IsAny<IEmailParameters>()))
            .ReturnsAsync(ServiceResult.Successful());

        // Set up rather than left to the mock's default, which hands back a null email and turns every
        // assertion about a render into a null reference on the way there.
        mock
            .Setup(x => x.RenderEmail(It.IsAny<IServiceRequest>(), It.IsAny<RenderEmailOptions>()))
            .ReturnsAsync(new RenderedEmail
            {
                BodyHtml = string.Empty,
                FromEmailAddress = string.Empty,
                FromName = string.Empty,
                Subject = string.Empty
            });

        return mock;
    }

    private static Currency CreateCurrency() => new()
    {
        Code = "GBP",
        Id = Guid.NewGuid(),
        Symbol = "£"
    };

    private static IHttpRequestContext CreateHttpRequestContext()
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.BaseUrl).Returns("https://test.local");
        return mock.Object;
    }

    private static Member CreateMember(string emailAddress = "member@example.com") => new()
    {
        EmailAddress = emailAddress,
        FirstName = "Test",
        Id = Guid.NewGuid(),
        LastName = "Member"
    };

    /* Answers for whichever ids it is asked about: the sends that format a date per recipient group them
       by culture and index the result by member id, so a bare mock throws before the email is built. */
    private static IMemberLocaleService CreateMemberLocaleService()
    {
        var mock = new Mock<IMemberLocaleService>();

        mock
            .Setup(x => x.GetCulture(It.IsAny<Guid>()))
            .ReturnsAsync(CultureInfo.InvariantCulture);

        mock
            .Setup(x => x.GetCultures(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> ids) =>
                (IReadOnlyDictionary<Guid, CultureInfo>)ids
                    .Distinct()
                    .ToDictionary(x => x, _ => CultureInfo.InvariantCulture));

        return mock.Object;
    }

    private static Payment CreatePayment() => new()
    {
        Amount = 12.34M,
        CreatedUtc = DateTime.UtcNow,
        CurrencyId = Guid.NewGuid(),
        Id = Guid.NewGuid(),
        MemberId = Guid.NewGuid(),
        Reference = "REF123"
    };

    private static IServiceRequest CreateServiceRequest() => new ServiceRequest
    {
        CurrentMemberOrDefault = null,
        Environment = EnvironmentType.Dev,
        HttpRequestContext = CreateHttpRequestContext(),
        Platform = PlatformType.Default
    };

    private static IReadOnlyCollection<INewTopic> CreateTopics(Guid memberId) =>
    [
        new NewMemberTopic
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Topic = "Test topic",
            TopicGroup = "Test topic group"
        }
    ];

    private static MemberEmailService CreateService(
        Mock<IEmailService> emailService,
        Mock<IUrlProvider> urlProvider,
        IUnitOfWork? unitOfWork = null)
    {
        var urlProviderFactory = new Mock<IUrlProviderFactory>();
        urlProviderFactory
            .Setup(x => x.Create(It.IsAny<IServiceRequest>(), It.IsAny<Chapter?>()))
            .Returns(urlProvider.Object);

        return new MemberEmailService(
            emailService.Object,
            urlProviderFactory.Object,
            unitOfWork ?? Mock.Of<IUnitOfWork>(),
            CreateMemberLocaleService(),
            // The real factory, not a mock: a bare mock hands back null parameters, which would make every
            // assertion about what a test email carries pass for the wrong reason.
            new TestEmailParametersFactory(urlProviderFactory.Object));
    }
}
