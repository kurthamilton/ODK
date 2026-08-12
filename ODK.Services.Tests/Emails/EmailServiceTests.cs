using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
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
            body: "<p>{test.url}</p>",
            parameters: new CustomEmailParameters
            {
                { "group.name", "Overridden" },
                { "test.url", "https://test.local/somewhere" }
            });

        // Assert
        sent.Subject.Should().Be("Overridden subject");
        sent.Body.Should().Contain("https://test.local/somewhere");
    }

    [Test]
    public static async Task SendEmail_TemplateUsingAudienceTitles_ResolvesThemFromSiteSettings()
    {
        // Arrange - a group that has never filled its settings form in inherits every title from the site.
        var sent = await SendTemplate(
            subject: "{memberTitle} subject",
            body: "<p>{adminTitle} - {title}</p>");

        // Assert - each is a template in its own right, so the group name inside them resolves too.
        sent.Subject.Should().Be("Test group members subject");
        sent.Body.Should().Contain("Test group admins - Test group");
    }

    [Test]
    public static async Task SendEmail_GroupSetsOneAudienceTitle_UsesItAndInheritsTheOther()
    {
        // Arrange - the two are inherited independently, so setting one must not drag the other along.
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{memberTitle} - {adminTitle}</p>",
            chapterEmailSettings: new ChapterEmailSettings
            {
                Id = Guid.NewGuid(),
                MemberTitle = "Our own wording"
            });

        // Assert
        sent.Body.Should().Contain("Our own wording - Test group admins");
    }

    [Test]
    public static async Task SendEmail_GroupTitleIsBlank_InheritsTheSites()
    {
        // Arrange - blank is how the form posts a box the group cleared, and it reads as unset rather than
        // as a title of nothing. Pinned because the failure is a silently empty email title.
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{memberTitle}</p>",
            chapterEmailSettings: new ChapterEmailSettings
            {
                Id = Guid.NewGuid(),
                MemberTitle = string.Empty
            });

        // Assert
        sent.Body.Should().Contain("Test group members");
    }

    [Test]
    public static async Task SendEmail_GroupSetsATitle_DoesNotChangeTheLegacyTitle()
    {
        // Arrange - {title} is site-wide: a group has no override of it, so its own wording stays out.
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{title}</p>",
            chapterEmailSettings: new ChapterEmailSettings
            {
                AdminTitle = "Our own wording",
                Id = Guid.NewGuid(),
                MemberTitle = "Our own wording"
            });

        // Assert
        sent.Body.Should().Contain("Test group");
        sent.Body.Should().NotContain("Our own wording");
    }

    [Test]
    public static async Task SendEventCommentEmail_SendsTheAdminTemplateToAdminsAndTheReplyTemplateToTheMember()
    {
        // Arrange - one send per audience, each reading its own template, so wording meant for admins
        // cannot reach members.
        var sent = await SendEventComment(adminReceivesCommentEmails: true, withReplyToMember: true);

        // Assert
        sent.Should().HaveCount(2);
        sent.Should().ContainSingle(x =>
            x.Subject == "Admin comment" && x.To.Single().Address == "admin@example.com");
        sent.Should().ContainSingle(x =>
            x.Subject == "Comment reply" && x.To.Single().Address == "member@example.com");
    }

    [Test]
    public static async Task SendEventCommentEmail_NoAdminReceivesThem_SendsOnlyTheReply()
    {
        // Arrange - a group whose admins have all opted out has nobody to send the admin copy to, so that
        // send is skipped rather than queued with no recipients.
        var sent = await SendEventComment(adminReceivesCommentEmails: false, withReplyToMember: true);

        // Assert
        sent.Should().ContainSingle();
        sent.Single().Subject.Should().Be("Comment reply");
    }

    [Test]
    public static async Task SendEventCommentEmail_NoReplyToMember_SendsOnlyTheAdminTemplate()
    {
        // Arrange - a top-level comment has nobody to notify of a reply.
        var sent = await SendEventComment(adminReceivesCommentEmails: true, withReplyToMember: false);

        // Assert
        sent.Should().ContainSingle();
        sent.Single().Subject.Should().Be("Admin comment");
    }

    private static async Task<IReadOnlyCollection<EmailClientEmail>> SendEventComment(
        bool adminReceivesCommentEmails,
        bool withReplyToMember)
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

        // Distinct subjects, so an assertion can tell which template each send used.
        context.Create(new Email
        {
            HtmlContent = "<p>admin</p>",
            RecipientType = EmailRecipientType.Admins,
            Subject = "Admin comment",
            Type = EmailType.EventComment
        });

        context.Create(new Email
        {
            HtmlContent = "<p>reply</p>",
            RecipientType = EmailRecipientType.Members,
            Subject = "Comment reply",
            Type = EmailType.EventCommentReply
        });

        var admin = context.CreateMember(afterCreate: x => x.EmailAddress = "admin@example.com");
        context.Create(new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            Member = admin,
            MemberId = admin.Id,
            ReceiveEventCommentEmails = adminReceivesCommentEmails,
            Role = ChapterAdminRole.Admin
        });

        var replyToMember = withReplyToMember
            ? context.CreateMember(afterCreate: x => x.EmailAddress = "member@example.com")
            : null;

        var sent = new List<EmailClientEmail>();
        var emailClient = new Mock<IEmailClient>();
        emailClient
            .Setup(x => x.SendEmail(It.IsAny<EmailClientEmail>()))
            .Callback<EmailClientEmail>(sent.Add)
            .ReturnsAsync(new SendEmailResult(true) { ExternalId = "external-id" });

        var service = CreateService(context, chapter, emailClient);

        var request = new ServiceRequest
        {
            CurrentMemberOrDefault = null,
            HttpRequestContext = Mock.Of<IHttpRequestContext>(),
            Platform = PlatformType.DrunkenKnitwits
        };

        // Act
        await service.SendEventCommentEmail(
            request,
            chapter,
            replyToMember,
            new EventComment { Id = Guid.NewGuid(), Text = "A comment" },
            parameters: null);

        return sent;
    }

    private static async Task<EmailClientEmail> SendTemplate(
        string subject,
        string body,
        IEmailParameters? parameters = null,
        ChapterEmailSettings? chapterEmailSettings = null)
    {
        using var context = new MockOdkContext();

        var chapter = context.Create(new Chapter
        {
            Id = Guid.NewGuid(),
            Name = "Test group",
            Platform = PlatformType.DrunkenKnitwits,
            Slug = "test-group"
        });

        // Each title is itself a template, so they are given one to prove they are interpolated rather
        // than passed through - the whole-list test below would pass on an empty string either way.
        context.Create(new SiteEmailSettings
        {
            AdminTitle = "{group.name} admins",
            FromEmailAddress = "noreply@example.com",
            FromName = "{group.name}",
            Id = Guid.NewGuid(),
            MemberTitle = "{group.name} members",
            Platform = PlatformType.DrunkenKnitwits,
            PlatformTitle = "Platform",
            Title = "{group.name}"
        });

        if (chapterEmailSettings != null)
        {
            chapterEmailSettings.ChapterId = chapter.Id;
            context.Create(chapterEmailSettings);
        }

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
