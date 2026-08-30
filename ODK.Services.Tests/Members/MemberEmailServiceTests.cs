using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
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
    public static async Task SendMemberImportInviteEmail_SendsInviteTemplateWithTheAcceptUrl()
    {
        /* Arrange - the page where an invitation is accepted, which the template names group.urls.join. The
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

    private static Chapter CreateChapter() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        Slug = "test-group"
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

    private static Member CreateMember() => new()
    {
        EmailAddress = "member@example.com",
        FirstName = "Test",
        Id = Guid.NewGuid(),
        LastName = "Member"
    };

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

    private static MemberEmailService CreateService(
        Mock<IEmailService> emailService,
        Mock<IUrlProvider> urlProvider,
        IUnitOfWork? unitOfWork = null)
    {
        var urlProviderFactory = new Mock<IUrlProviderFactory>();
        urlProviderFactory
            .Setup(x => x.Create(It.IsAny<IServiceRequest>()))
            .ReturnsAsync(urlProvider.Object);

        return new MemberEmailService(
            emailService.Object,
            urlProviderFactory.Object,
            unitOfWork ?? Mock.Of<IUnitOfWork>(),
            Mock.Of<IMemberLocaleService>(),
            // The real factory, not a mock: a bare mock hands back null parameters, which would make every
            // assertion about what a test email carries pass for the wrong reason.
            new TestEmailParametersFactory(urlProviderFactory.Object));
    }
}
