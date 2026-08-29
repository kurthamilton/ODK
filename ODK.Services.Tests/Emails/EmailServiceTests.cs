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
        //
        // group.name is the group's full name, from its own platform: this is a Drunken Knitwits chapter, so
        // it carries the suffix whichever platform's request triggered the send.
        var sent = await SendTemplate(
            subject: "{group.name} subject",
            body: "<p>{group.name} - {group.url}</p>");

        // Assert
        sent.Subject.Should().Be("Test group Drunken Knitwits subject");
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
            body: "<p>{GROUP.NAME}</p>");

        // Assert
        sent.Subject.Should().Be("Test group Drunken Knitwits subject");
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
    public static async Task SendEmail_MemberEmail_TitleIsTheSitesMemberTitle()
    {
        // Arrange - a group that has never filled its settings form in takes every title from the site.
        var sent = await SendTemplate(
            subject: "{title} subject",
            body: "<p>{title}</p>",
            recipientType: EmailRecipientType.Members);

        // Assert - the title is a template in its own right, so the group name inside it resolves too.
        sent.Subject.Should().Be("Test group Drunken Knitwits members subject");
        sent.Body.Should().Contain("Test group Drunken Knitwits members");
    }

    [Test]
    public static async Task SendEmail_AdminEmail_TitleIsTheSitesAdminTitle()
    {
        // Arrange - the audience is the email's own, so the same template yields different wording.
        var sent = await SendTemplate(
            subject: "{title} subject",
            body: "<p>{title}</p>",
            recipientType: EmailRecipientType.Admins);

        // Assert
        sent.Subject.Should().Be("Test group Drunken Knitwits admins subject");
        sent.Body.Should().Contain("Test group Drunken Knitwits admins");
    }

    [Test]
    public static async Task SendEmail_GroupSetsTheTitleForThisAudience_UsesTheGroupsWording()
    {
        // Arrange
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{title}</p>",
            recipientType: EmailRecipientType.Members,
            chapterEmailSettings: new ChapterEmailSettings
            {
                Id = Guid.NewGuid(),
                MemberTitle = "Our own wording"
            });

        // Assert
        sent.Body.Should().Contain("Our own wording");
    }

    [Test]
    public static async Task SendEmail_GroupSetsTheTitleForTheOtherAudience_InheritsTheSites()
    {
        // Arrange - the two audiences are inherited independently, so a group setting one leaves the other
        // alone. An admin email must not pick up wording written for members.
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{title}</p>",
            recipientType: EmailRecipientType.Admins,
            chapterEmailSettings: new ChapterEmailSettings
            {
                Id = Guid.NewGuid(),
                MemberTitle = "Our own wording"
            });

        // Assert
        sent.Body.Should().Contain("Test group Drunken Knitwits admins");
        sent.Body.Should().NotContain("Our own wording");
    }

    [Test]
    public static async Task SendEmail_GroupOverridesSubjectOnly_SendsTheSitesBody()
    {
        // Arrange - subject and body are overridden independently, so overriding one must leave the other
        // sending the site's rather than sending nothing.
        var sent = await SendTemplate(
            subject: "Site subject",
            body: "<p>Site body</p>",
            chapterEmail: new ChapterEmail
            {
                Id = Guid.NewGuid(),
                Subject = "Our own subject"
            });

        // Assert
        sent.Subject.Should().Be("Our own subject");
        sent.Body.Should().Contain("Site body");
    }

    [Test]
    public static async Task SendEmail_GroupOverridesBodyOnly_SendsTheSitesSubject()
    {
        // Arrange
        var sent = await SendTemplate(
            subject: "Site subject",
            body: "<p>Site body</p>",
            chapterEmail: new ChapterEmail
            {
                Id = Guid.NewGuid(),
                HtmlContent = "<p>Our own body</p>"
            });

        // Assert
        sent.Subject.Should().Be("Site subject");
        sent.Body.Should().Contain("Our own body");
        sent.Body.Should().NotContain("Site body");
    }

    [Test]
    public static async Task SendEmail_GroupOverrideFieldIsEmpty_InheritsTheSites()
    {
        // Arrange - an empty override reads as unset rather than as an override with nothing in it. Pinned
        // because the failure is a silently empty subject on every send.
        var sent = await SendTemplate(
            subject: "Site subject",
            body: "<p>Site body</p>",
            chapterEmail: new ChapterEmail
            {
                HtmlContent = "<p>Our own body</p>",
                Id = Guid.NewGuid(),
                Subject = string.Empty
            });

        // Assert
        sent.Subject.Should().Be("Site subject");
    }

    [Test]
    public static async Task SendEmail_GroupTitleIsBlank_InheritsTheSites()
    {
        // Arrange - blank is how the form posts a box the group cleared, and it reads as unset rather than
        // as a title of nothing. Pinned because the failure is a silently empty email title.
        var sent = await SendTemplate(
            subject: "subject",
            body: "<p>{title}</p>",
            recipientType: EmailRecipientType.Members,
            chapterEmailSettings: new ChapterEmailSettings
            {
                Id = Guid.NewGuid(),
                MemberTitle = string.Empty
            });

        // Assert
        sent.Body.Should().Contain("Test group Drunken Knitwits members");
    }

    [Test]
    public static async Task SendEmail_AdHocSend_TitleFollowsTheAudienceItDeclares()
    {
        // Arrange - a send carrying its own subject and body has no email row to take an audience from, so
        // the one it states is what the title resolves through. Nothing falls back to the site-wide title.
        var admin = await SendAdHoc(EmailRecipientType.Admins);
        var member = await SendAdHoc(EmailRecipientType.Members);

        // Assert
        admin.Subject.Should().Be("Test group Drunken Knitwits admins subject");
        member.Subject.Should().Be("Test group Drunken Knitwits members subject");
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
            Id = Guid.NewGuid(),
            Platform = PlatformType.DrunkenKnitwits
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
            Environment = EnvironmentType.Dev,
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

    /* The subject/body overload rather than a template: it takes Type's default of Layout, so there is no
       body email and the recipient type it is given is the only audience available. */
    private static async Task<EmailClientEmail> SendAdHoc(EmailRecipientType recipientType)
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
            AdminTitle = "{group.name} admins",
            FromEmailAddress = "noreply@example.com",
            Id = Guid.NewGuid(),
            MemberTitle = "{group.name} members",
            Platform = PlatformType.DrunkenKnitwits
        });

        context.Create(new Email
        {
            HtmlContent = "{body}",
            Subject = string.Empty,
            Type = EmailType.Layout
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
            Environment = EnvironmentType.Dev,
            HttpRequestContext = Mock.Of<IHttpRequestContext>(),
            Platform = PlatformType.DrunkenKnitwits
        };

        // Act
        await service.SendEmail(
            request,
            chapter,
            [new EmailAddressee("member@example.com", "Test Member")],
            "{title} subject",
            "<p>{title}</p>",
            recipientType);

        sent.Should().NotBeNull();
        return sent!;
    }

    private static async Task<EmailClientEmail> SendTemplate(
        string subject,
        string body,
        IEmailParameters? parameters = null,
        ChapterEmailSettings? chapterEmailSettings = null,
        EmailRecipientType recipientType = EmailRecipientType.Members,
        ChapterEmail? chapterEmail = null)
    {
        using var context = new MockOdkContext();

        var chapter = context.Create(new Chapter
        {
            Id = Guid.NewGuid(),
            Name = "Test group",
            Platform = PlatformType.DrunkenKnitwits,
            Slug = "test-group"
        });

        /* Each title is a template of its own, and the two are given distinguishable wording so an
           assertion can tell which one an email resolved to. */
        context.Create(new SiteEmailSettings
        {
            AdminTitle = "{group.name} admins",
            FromEmailAddress = "noreply@example.com",
            Id = Guid.NewGuid(),
            MemberTitle = "{group.name} members",
            Platform = PlatformType.DrunkenKnitwits
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
            RecipientType = recipientType,
            Subject = subject,
            Type = EmailType.NewMember
        });

        if (chapterEmail != null)
        {
            chapterEmail.ChapterId = chapter.Id;
            chapterEmail.Type = EmailType.NewMember;
            context.Create(chapterEmail);
        }

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
            Environment = EnvironmentType.Dev,
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
            MockUnitOfWorkFactory.Create(context),
            emailClient.Object,
            urlProviderFactory.Object,
            new MockBackgroundTaskService(),
            Mock.Of<ILoggingService>(),
            TestPlatformProvider.Create(),
            new EmailServiceSettings
            {
                DefaultBodyBackground = "#fff",
                DefaultBodyColor = "#000",
                DefaultHeaderBackground = "#fff",
                DefaultHeaderColor = "#000"
            });
    }
}
