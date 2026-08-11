using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Emails;
using ODK.Services.Logging;
using ODK.Services.Tests.Helpers;
using ODK.Services.Web;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailServiceTests
{
    [Test]
    public static async Task SendEmail_TemplateUsingChapterParameters_LeavesThemUnresolved()
    {
        // Arrange - chapter.* is no longer supplied. Pinned rather than left untested, because the
        // failure is silent: a template on the old name renders the braces to the member verbatim.
        var sent = await SendTemplate(
            subject: "{chapter.name} subject",
            body: "<p>{chapter.fullName} - {chapter.baseurl}</p>");

        // Assert
        sent.Subject.Should().Be("{chapter.name} subject");
        sent.Body.Should().Contain("{chapter.fullName} - {chapter.baseurl}");
    }

    [Test]
    public static async Task SendEmail_TemplateUsingGroupParameters_ResolvesThem()
    {
        // Arrange - the parameters the app supplies. An unresolved token renders as literal braces in
        // a real email, so assert on the substituted value rather than merely "no exception".
        var sent = await SendTemplate(
            subject: "{group.name} subject",
            body: "<p>{group.fullname} - {group.url}</p>");

        // Assert
        sent.Subject.Should().Be("Test group subject");
        sent.Body.Should().Contain("Test group Drunken Knitwits - https://test.local/groups/test-group");
    }

    [Test]
    public static async Task SendEmail_TemplateUsingEveryOfferedPlaceholder_ResolvesAllOfThem()
    {
        // Arrange - the email admin pages offer EmailParameters.Names as insertable buttons. A name on
        // that list the send path does not actually supply reaches the member as literal braces, so
        // build a template out of the whole list rather than trusting the two to stay in step.
        var body = string.Join(" ", EmailParameters.Names.Select(x => $"{{{x}}}"));

        // Act
        var sent = await SendTemplate(subject: "subject", body: body);

        // Assert
        sent.Body.Should().NotContain("{");
    }

    [Test]
    public static async Task SendEmail_TokenCasingDiffersFromTheParameter_StillResolvesIt()
    {
        // Arrange - templates are hand-authored, by us in a migration or by an admin in a textarea, so
        // matching is case-insensitive. Pinned because the alternative fails silently in a sent email.
        var sent = await SendTemplate(
            subject: "{Group.Name} subject",
            body: "<p>{GROUP.FULLNAME}</p>");

        // Assert
        sent.Subject.Should().Be("Test group subject");
        sent.Body.Should().Contain("Test group Drunken Knitwits");
    }

    [Test]
    public static async Task SendEmail_CallerSuppliesACoreParameter_TheirValueWins()
    {
        // Arrange - the core values are defaults. An email that supplies its own is the more specific
        // answer, so the merge overwrites rather than filling gaps.
        var sent = await SendTemplate(
            subject: "{group.name} subject",
            body: "<p>{url}</p>",
            parameters: new CustomEmailParameters
            {
                { "group.name", "Overridden" },
                { "url", "https://test.local/somewhere" }
            });

        // Assert
        sent.Subject.Should().Be("Overridden subject");
        sent.Body.Should().Contain("https://test.local/somewhere");
    }

    private static async Task<EmailClientEmail> SendTemplate(
        string subject,
        string body,
        IEmailParameters? parameters = null)
    {
        using var context = new MockOdkContext();

        var chapter = context.Create(new Chapter
        {
            Id = Guid.NewGuid(),
            Name = "Test group",
            Platform = PlatformType.DrunkenKnitwits,
            Slug = "test-group"
        });

        context.Create(new SiteEmailSettings
        {
            FromEmailAddress = "noreply@example.com",
            FromName = "{group.name}",
            Id = Guid.NewGuid(),
            Platform = PlatformType.DrunkenKnitwits,
            PlatformTitle = "Platform",
            Title = "{group.name}"
        });

        context.Create(new Email
        {
            HtmlContent = "{body}",
            Subject = string.Empty,
            Type = EmailType.Layout
        });

        context.Create(new Email
        {
            HtmlContent = body,
            Subject = subject,
            Type = EmailType.NewMember
        });

        EmailClientEmail? sent = null;
        var emailClient = new Mock<IEmailClient>();
        emailClient
            .Setup(x => x.SendEmail(It.IsAny<EmailClientEmail>()))
            .Callback<EmailClientEmail>(x => sent = x)
            .ReturnsAsync(new SendEmailResult(true) { ExternalId = "external-id" });

        var service = CreateService(context, chapter, emailClient);

        var request = new ServiceRequest
        {
            CurrentMemberOrDefault = null,
            HttpRequestContext = Mock.Of<IHttpRequestContext>(),
            Platform = PlatformType.DrunkenKnitwits
        };

        // Act
        await service.SendEmail(
            request,
            chapter,
            new EmailAddressee("member@example.com", "Test Member"),
            EmailType.NewMember,
            parameters);

        // Assert - the background task service runs the send synchronously, so the client has the
        // fully rendered email by the time this returns.
        sent.Should().NotBeNull();
        return sent!;
    }

    private static EmailService CreateService(
        MockOdkContext context,
        Chapter chapter,
        Mock<IEmailClient> emailClient)
    {
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.Setup(x => x.BaseUrl()).Returns("https://test.local");
        urlProvider.Setup(x => x.GroupUrl(chapter)).Returns("https://test.local/groups/test-group");

        var urlProviderFactory = new Mock<IUrlProviderFactory>();
        urlProviderFactory
            .Setup(x => x.Create(It.IsAny<IServiceRequest>()))
            .ReturnsAsync(urlProvider.Object);

        return new EmailService(
            MockUnitOfWork.Create(context),
            emailClient.Object,
            urlProviderFactory.Object,
            new MockBackgroundTaskService(),
            Mock.Of<ILoggingService>(),
            new EmailServiceSettings
            {
                DefaultBodyBackground = "#fff",
                DefaultBodyColor = "#000",
                DefaultHeaderBackground = "#fff",
                DefaultHeaderColor = "#000"
            });
    }
}
