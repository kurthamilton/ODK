using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Services.Authorization;
using ODK.Services.Emails;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;
using ODK.Services.Web;

namespace ODK.Services.Tests.Emails;

[Parallelizable]
public static class EmailAdminServiceTests
{
    private const string HtmlFailure = "Malformed HTML at line 1";

    private const EmailType Type = EmailType.NewMember;

    [Test]
    public static async Task GetChapterEmails_TakesTheRecipientTypeFromTheSiteEmail()
    {
        // Arrange - a ChapterEmail carries no recipient type, so the list reads it from the site's row. The
        // group has overridden this template, which is the case where there is something else to read.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context, EmailRecipientType.Admins);
        CreateSiteEmailSettings(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Custom</p>",
            Id = Guid.NewGuid(),
            Subject = "Custom",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        var email = result.Emails.Single();
        email.RecipientType.Should().Be(EmailRecipientType.Admins);
        email.Email.IsDefault().Should().BeFalse();
    }

    [Test]
    public static async Task GetEmail_ResolvesOnlyWhatTheSiteTemplateKnows()
    {
        // Arrange - the platform is the same whatever the email is about, so its URL has a value. A group's
        // does not: this is the template every group starts from, so it belongs to no one group.
        using var context = new MockOdkContext();
        var currentMember = context.CreateMember(siteAdmin: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetEmail(CreateMemberRequest(currentMember), Type);

        // Assert
        result.Parameters.Single(x => x.Name == "platform.url").Value.Should().NotBeNull();
        result.Parameters.Single(x => x.Name == "group.name").Value.Should().BeNull();
        result.Parameters.Single(x => x.Name == "title").Value.Should().Be("Site members");
    }

    [Test]
    public static async Task GetChapterEmails_WithoutAnOverride_ReportsNothingCustomised()
    {
        // Arrange - the list names which fields a group overrides, so a template it has not touched must
        // report none. The row standing in for an un-customised template carries no wording: filling it with
        // the site's would make every template read as fully customised.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        var email = result.Emails.Single().Email;
        email.OverridesSubject.Should().BeFalse();
        email.OverridesContent.Should().BeFalse();
        email.OverridesAnything().Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterEmails_WithOneFieldOverridden_ReportsOnlyThatField()
    {
        // Arrange - subject and body are reported independently, so overriding one must not report both.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            Subject = "Custom",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        var email = result.Emails.Single().Email;
        email.OverridesSubject.Should().BeTrue();
        email.OverridesContent.Should().BeFalse();
    }

    [Test]
    public static async Task GetChapterEmails_ReturnsTheGroupsSettings()
    {
        // Arrange - the settings form is rendered from these, so the page needs them alongside the list.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);
        context.Create(new ChapterEmailSettings
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            MemberTitle = "Our own wording"
        });

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert - the site's titles come through too, since the form shows them beside the group's boxes.
        result.Settings.Should().NotBeNull();
        result.Settings.MemberTitle.Should().Be("Our own wording");
        result.SiteMemberTitle.Should().Be("Site members");
        result.SiteAdminTitle.Should().Be("Site admins");
        result.CanEdit.Should().BeTrue();
    }

    [Test]
    public static async Task GetChapterEmails_WithoutTheFeature_StillListsTheTemplates()
    {
        // Arrange - the list is navigation, not something the feature withholds: an owner sees which
        // templates exist, and which are already customised, before deciding whether to upgrade.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmails(CreateRequest(chapter, currentMember));

        // Assert
        result.Emails.Should().ContainSingle(x => x.Email.Type == Type);
        result.CanEdit.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithTheFeature_Succeeds()
    {
        // Arrange
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterEmail_SubjectOnly_LeavesTheBodyInheriting()
    {
        // Arrange - overriding one field must not store the other as an empty override, which would send a
        // blank body rather than the site's.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel(htmlContent: null));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id && x.Type == Type);
        stored.Subject.Should().Be("Updated");
        stored.HtmlContent.Should().BeNull();
        stored.OverridesSubject.Should().BeTrue();
        stored.OverridesContent.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterEmail_BlankField_StoresItAsUnsetRatherThanEmpty()
    {
        // Arrange - blank is what the form posts for a box the group cleared. Stored as null so the row says
        // the group has not overridden the field, which is also what the send path falls back on.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel(subject: "   "));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id && x.Type == Type);
        stored.Subject.Should().BeNull();
        stored.HtmlContent.Should().Be("<p>Updated</p>");
    }

    [Test]
    public static async Task UpdateChapterEmail_BothFieldsBlank_RemovesTheOverride()
    {
        // Arrange - clearing both is how a group goes back to the standard email, so no row is left behind
        // overriding nothing (which would still show the email as customised).
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel(subject: null, htmlContent: null));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<ChapterEmail>().Should().BeEmpty();
    }

    [Test]
    public static async Task GetChapterEmail_WithoutAnOverride_LeavesTheWordingUnsetAndSuppliesTheSites()
    {
        // Arrange - the form shows the site's wording as what an un-overridden field sends, rather than
        // pre-filling the group's boxes with a copy of it.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmail(CreateRequest(chapter, currentMember), Type);

        // Assert
        result.Email.Subject.Should().BeNull();
        result.Email.HtmlContent.Should().BeNull();
        result.Email.Type.Should().Be(Type);
        result.SiteEmail.Subject.Should().Be("Standard");
        result.SiteEmail.HtmlContent.Should().Be("<p>Standard</p>");
    }

    [Test]
    public static async Task GetChapterEmail_ResolvesWhatIsKnownAboutTheGroup()
    {
        // Arrange - the group is fixed on this page, so an author can see what its parameters put in the
        // email. What the email is about is not, so those stay unresolved.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        CreateSiteEmailSettings(context);

        var service = CreateService(context);

        // Act
        var result = await service.GetChapterEmail(CreateRequest(chapter, currentMember), Type);

        /* Assert - resolved from the same type the send path fills in, so these are the values it would use.
           group.name is the group's full name, taken from the group's own platform rather than the request's,
           so the table shows what an email would carry. */
        Value(result, "group.name").Should().Be(chapter.FullName);
        Value(result, "group.url").Should().NotBeNull();

        // The title comes from the audience and the group's settings rather than from that dictionary.
        Value(result, "title").Should().Be("Site members");

        // Unresolved because it stands for the member being emailed.
        Value(result, "member.firstName").Should().BeNull();

        /* Also unresolved, though it could be: it reads like a group parameter but belongs to the email type,
           which is handed the URL by its caller rather than working it out. Resolving it here would mean
           building it a second way, so it stays blank until that is worth doing. */
        Value(result, "group.urls.events").Should().BeNull();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_ClearsAnExistingOverride()
    {
        // Arrange - a group that cannot write wording can still stop customising, which is the state it would
        // be in had it never customised. Otherwise losing the feature would strand it with wording it could
        // neither change nor remove.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing",
            Type = Type
        });

        var service = CreateService(context);

        // Act - the body posts back unchanged, as a locked field does; only the subject is cleared.
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember),
            Type,
            CreateUpdateModel(subject: null, htmlContent: "<p>Existing</p>"));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().BeNull();
        stored.HtmlContent.Should().Be("<p>Existing</p>");
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_TurningOffOneField_KeepsTheOther()
    {
        // Arrange - a group that customised both fields and then lost the feature turns the body off. Both
        // fields are locked, so neither posts wording; only the flags say which is still overridden. Reading
        // the absent subject as "cleared" removes wording the group never touched, and empties the row.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing body</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing subject",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember),
            Type,
            CreateUpdateModel(
                subject: null,
                htmlContent: null,
                overrideSubject: true,
                overrideHtmlContent: false));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().Be("Existing subject");
        stored.HtmlContent.Should().BeNull();
    }

    [Test]
    public static async Task UpdateChapterEmail_StoredBodyWouldNotValidate_TurningOffTheSubjectStillSaves()
    {
        // Arrange - a body stored before today's markup rules, which now fails them. Changing the subject
        // does not touch it, so the save must not be judged on it: the group would be told to fix wording
        // this save is not writing, and a group without the feature could not fix it at all.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p style=\"text-align: center;\">Existing body</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing subject",
            Type = Type
        });

        // Rejects any markup it is given, standing in for the stored body failing the current rules.
        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act - turn the subject off and leave the body customised.
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember),
            Type,
            CreateUpdateModel(
                subject: null,
                htmlContent: null,
                overrideSubject: false,
                overrideHtmlContent: true));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().BeNull();
        stored.HtmlContent.Should().Be("<p style=\"text-align: center;\">Existing body</p>");
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_SavingUnchanged_KeepsBoth()
    {
        // Arrange - every field is locked without the feature, so a save that changes nothing posts no
        // wording at all. Pressing Update must leave the override exactly as it was rather than wiping it.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing body</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing subject",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember),
            Type,
            CreateUpdateModel(
                subject: null,
                htmlContent: null,
                overrideSubject: true,
                overrideHtmlContent: true));

        // Assert
        result.Success.Should().BeTrue();

        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().Be("Existing subject");
        stored.HtmlContent.Should().Be("<p>Existing body</p>");
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_ReturnsFailure()
    {
        // Arrange - the form locks what cannot be written, but that is only presentation. This is the guard,
        // and it is what a posted form has to get past.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateChapterEmail_WithoutTheFeature_LeavesTheExistingOverrideAlone()
    {
        // Arrange - a group that customised an email before its subscription changed keeps sending what it
        // had. A post carrying different wording is refused outright, leaving the row as it was - unlike a
        // post that only clears a field, which is allowed.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);
        context.Create(new ChapterEmail
        {
            ChapterId = chapter.Id,
            HtmlContent = "<p>Existing</p>",
            Id = Guid.NewGuid(),
            Subject = "Existing",
            Type = Type
        });

        var service = CreateService(context);

        // Act
        await service.UpdateChapterEmail(CreateRequest(chapter, currentMember), Type, CreateUpdateModel());

        // Assert
        var stored = context.Set<ChapterEmail>().Single(x => x.ChapterId == chapter.Id);
        stored.Subject.Should().Be("Existing");
        stored.HtmlContent.Should().Be("<p>Existing</p>");
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_BlankTitle_StoresItAsUnset()
    {
        // Arrange - blank is how the form posts a box the group cleared. Stored as null so the row says
        // the group has not set a title, which is what makes it inherit the site's again.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        context.Create(new ChapterEmailSettings
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            MemberTitle = "Previous wording"
        });

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = null,
                MemberTitle = "   "
            });

        // Assert
        result.Success.Should().BeTrue();
        var stored = context.Set<ChapterEmailSettings>().Single(x => x.ChapterId == chapter.Id);
        stored.MemberTitle.Should().BeNull();
        stored.AdminTitle.Should().BeNull();
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_WithTheFeature_StoresTheTitles()
    {
        // Arrange - no row exists until the group saves the form for the first time.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = "Admin wording",
                MemberTitle = "Member wording"
            });

        // Assert
        result.Success.Should().BeTrue();
        var stored = context.Set<ChapterEmailSettings>().Single(x => x.ChapterId == chapter.Id);
        stored.AdminTitle.Should().Be("Admin wording");
        stored.MemberTitle.Should().Be("Member wording");
    }

    [Test]
    public static async Task UpdateChapterEmailSettings_WithoutTheFeature_ReturnsFailure()
    {
        // Arrange - the form renders read-only without the feature, but that is only presentation. This is
        // the guard, and it is what a posted form has to get past.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        var result = await service.UpdateChapterEmailSettings(
            CreateRequest(chapter, currentMember),
            new ChapterEmailSettingsUpdateModel
            {
                AdminTitle = "Admin wording",
                MemberTitle = "Member wording"
            });

        // Assert
        result.Success.Should().BeFalse();
        context.Set<ChapterEmailSettings>().Should().BeEmpty();
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_LayoutEmail_SkipsTheCheck()
    {
        // Arrange - the layout is the full HTML document the other emails render into, so the allow-list
        // tuned for rich text would reject it outright. The editor must not flag what the save accepts.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), EmailType.Layout, "<html><body></body></html>");

        // Assert
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task UpdateChapterEmail_WritingTheBody_StillValidatesIt()
    {
        // Arrange - the counterpart: skipping the check for an untouched field must not skip it for one the
        // save is writing, or malformed markup would go in unchecked.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.UpdateChapterEmail(
            CreateRequest(chapter, currentMember), Type, CreateUpdateModel(htmlContent: "<p>Malformed</p"));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_MalformedHtml_ReturnsFailure()
    {
        // Arrange
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_NotChapterAdmin_Throws()
    {
        // Arrange - the endpoint takes the content the caller is editing, so it has to be as authorised
        // as the save is: without this, any member could use it to probe another group's admin routes.
        using var context = new MockOdkContext();
        var (chapter, _) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);
        var outsider = context.CreateMember();

        var service = CreateService(context);

        // Act
        var act = async () => await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, outsider), Type, "<p>Valid</p>");

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_WithoutTheFeature_StillValidates()
    {
        // Arrange - deliberately not gated on the feature, unlike UpdateChapterEmail. Those refusals are
        // about whether a template may be saved at all, and answering them here would put "cannot be
        // customised" under a field as if it were a markup error.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: false);
        CreateSiteEmail(context);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = await service.ValidateChapterEmailHtml(
            CreateRequest(chapter, currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static async Task ValidateChapterEmailHtml_WritesNothing()
    {
        // Arrange - the editor calls this while the admin types, so it has to be free of side effects:
        // content that never gets submitted must not leave a saved override behind.
        using var context = new MockOdkContext();
        var (chapter, currentMember) = CreateChapter(context, withFeature: true);
        CreateSiteEmail(context);

        var service = CreateService(context);

        // Act
        await service.ValidateChapterEmailHtml(CreateRequest(chapter, currentMember), Type, "<p>Valid</p>");

        // Assert
        context.Set<ChapterEmail>().Should().BeEmpty();
    }

    [Test]
    public static void ValidateEmailHtml_MalformedHtml_ReturnsFailure()
    {
        // Arrange
        using var context = new MockOdkContext();
        var currentMember = context.CreateMember(siteAdmin: true);

        var service = CreateService(context, CreateHtmlValidator(ServiceResult.Failure(HtmlFailure)));

        // Act
        var result = service.ValidateEmailHtml(
            CreateMemberRequest(currentMember), Type, "<p>Malformed</p");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(HtmlFailure);
    }

    [Test]
    public static void ValidateEmailHtml_NotSiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        var currentMember = context.CreateMember();

        var service = CreateService(context);

        // Act
        var act = () => service.ValidateEmailHtml(CreateMemberRequest(currentMember), Type, "<p>Valid</p>");

        // Assert
        act.Should().Throw<OdkNotAuthorizedException>();
    }

    private static (Chapter Chapter, Member CurrentMember) CreateChapter(
        MockOdkContext context, bool withFeature)
    {
        var currentMember = context.CreateMember();

        var chapter = context.CreateChapter(
            adminMembers: [currentMember],
            siteSubscription: context.CreateSiteSubscription(
                features: withFeature ? [SiteFeatureType.CustomEmails] : []));

        return (chapter, currentMember);
    }

    // Set up explicitly rather than left bare: a Mock.Of with no configured return hands back null from
    // Validate, which reads as a pass and turns every failure case into a false pass.
    private static IHtmlValidator CreateHtmlValidator(ServiceResult result) => Mock.Of<IHtmlValidator>(x =>
        x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()) == result);

    private static IHttpRequestContext CreateHttpRequestContext()
    {
        var mock = new Mock<IHttpRequestContext>();
        mock.Setup(x => x.BaseUrl).Returns("https://test.local");
        return mock.Object;
    }

    private static IMemberServiceRequest CreateMemberRequest(Member currentMember)
    {
        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(currentMember);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        mock.Setup(x => x.HttpRequestContext).Returns(CreateHttpRequestContext());
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        return mock.Object;
    }

    private static IMemberChapterAdminServiceRequest CreateRequest(Chapter chapter, Member currentMember)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();
        mock.Setup(x => x.Chapter).Returns(chapter);
        mock.Setup(x => x.CurrentMember).Returns(currentMember);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        mock.Setup(x => x.HttpRequestContext).Returns(CreateHttpRequestContext());
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        mock.Setup(x => x.Securable).Returns(ChapterAdminSecurable.Emails);
        return mock.Object;
    }

    private static EmailAdminService CreateService(
        MockOdkContext context, IHtmlValidator? htmlValidator = null) => new(
        MockUnitOfWorkFactory.Create(context),
        Mock.Of<IMemberEmailService>(),
        // The real one, not a mock: it has no dependencies and is a pure function over the arranged
        // subscription features. A bare mock returns false from every check, which turns the
        // with-the-feature cases into false passes.
        new AuthorizationService(),
        htmlValidator ?? CreateHtmlValidator(ServiceResult.Successful()),
        CreateUrlProviderFactory(),
        new SiteSubscriptionCooldown(months: 0));

    // Returns a URL for anything asked of it: a bare mock hands back null, and a null group URL would read
    // as a parameter with no value rather than one this page can resolve.
    private static IUrlProviderFactory CreateUrlProviderFactory()
    {
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider.SetReturnsDefault("https://test.local/somewhere");

        return Mock.Of<IUrlProviderFactory>(x =>
            x.Create(It.IsAny<IServiceRequest>()) == Task.FromResult(urlProvider.Object));
    }

    private static void CreateSiteEmail(
        MockOdkContext context,
        EmailRecipientType recipientType = EmailRecipientType.Members) => context.Create(new Email
    {
        HtmlContent = "<p>Standard</p>",
        IsGroupEmail = true,
        RecipientType = recipientType,
        Subject = "Standard",
        Type = Type
    });

    /* The chapter email pages read the site's titles, to show a group what leaving a box empty gives it, so
       the settings row has to exist for them to load at all. */
    private static void CreateSiteEmailSettings(MockOdkContext context) => context.Create(new SiteEmailSettings
    {
        AdminTitle = "Site admins",
        FromEmailAddress = "noreply@example.com",
        Id = Guid.NewGuid(),
        MemberTitle = "Site members",
        Platform = PlatformType.Default
    });

    private static string? Value(ChapterEmailAdminPageViewModel result, string name) =>
        result.Parameters.Single(x => x.Name == name).Value;

    /* The override flags default to whether wording was supplied, which is what the form posts when every
       field is editable. A test covering a locked field passes them explicitly - that is the case where the
       two part company, because a locked field posts no wording while still being overridden. */
    private static ChapterEmailUpdateModel CreateUpdateModel(
        string? subject = "Updated",
        string? htmlContent = "<p>Updated</p>",
        bool? overrideSubject = null,
        bool? overrideHtmlContent = null) => new()
    {
        HtmlContent = htmlContent,
        OverrideHtmlContent = overrideHtmlContent ?? !string.IsNullOrWhiteSpace(htmlContent),
        OverrideSubject = overrideSubject ?? !string.IsNullOrWhiteSpace(subject),
        Subject = subject
    };
}
