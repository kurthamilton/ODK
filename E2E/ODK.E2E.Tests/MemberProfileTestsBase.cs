using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter member-profile-property scenarios (the per-chapter questions members answer), written once
/// and run against both platforms. Concrete per-platform fixtures supply the platform base URL +
/// category and the platform-specific provisioning (owner+chapter, join-with-answers) and route building.
/// Member-facing pages and admin forms are shared across platforms, so the scenario bodies are identical.
/// </summary>
public abstract class MemberProfileTestsBase : OdkPageTest
{
    private static ChapterPropertyDataHelper ChapterProperties => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberPropertyDataHelper MemberProperties => new(E2ESettings.ConnectionString);

    [Test]
    [Category("ChapterMembershipWorkflows")]
    public async Task JoinChapter_MissingRequiredProperty_CannotSubmit()
    {
        // Arrange - a chapter with one required and one optional profile question.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("required"), required: true);
        var optionalId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("optional"));

        // Act - attempt to join answering only the optional question, leaving the required one blank.
        var joined = await TryJoinChapterWithoutRequired(
            group, new Dictionary<Guid, string> { [optionalId] = "An optional answer" });

        // Assert - the join is blocked.
        joined.Should().BeFalse();
    }

    [Test]
    [Category("ChapterMembershipWorkflows")]
    public async Task JoinChapter_WithProperties_PersistsAnswers()
    {
        // Arrange - a chapter with a required and an optional profile question.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var requiredId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("required"), required: true);
        var optionalId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("optional"));

        const string requiredAnswer = "Required answer";
        const string optionalAnswer = "Optional answer";

        // Act - join answering both (filling the fields also proves they're visible on the form).
        var member = await JoinChapterWithProperties(group, new Dictionary<Guid, string>
        {
            [requiredId] = requiredAnswer,
            [optionalId] = optionalAnswer
        });

        // Assert - both answers are persisted.
        (await MemberProperties.GetValue(member.Email, requiredId)).Should().Be(requiredAnswer);
        (await MemberProperties.GetValue(member.Email, optionalId)).Should().Be(optionalAnswer);
    }

    [Test]
    public async Task MemberPage_AfterProfileUpdate_ShowsUpdatedAnswer()
    {
        // Arrange - a member who answered a profile question when joining.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var label = PropertyLabel("question");
        var propertyId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, label);
        var member = await JoinChapterWithProperties(
            group, new Dictionary<Guid, string> { [propertyId] = "Original answer" });
        var memberId = await Members.GetMemberId(member.Email);

        const string updatedAnswer = "Updated answer";

        // Act - the member updates their answer, then views their profile page.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        await new ProfileUpdatePage(Page).UpdateProperty(routes.ProfileUpdate, propertyId, updatedAnswer);
        var shown = await new MemberProfilePage(Page).GetAnswer(routes.MemberPage(memberId), label);

        // Assert - the member page reflects the update.
        shown.Should().Be(updatedAnswer);
    }

    [Test]
    public async Task MemberPage_AsMember_ShowsAnswer()
    {
        // Arrange - a member who answered a profile question when joining.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var label = PropertyLabel("question");
        var propertyId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, label);
        const string answer = "Visible answer";
        var member = await JoinChapterWithProperties(
            group, new Dictionary<Guid, string> { [propertyId] = answer });
        var memberId = await Members.GetMemberId(member.Email);

        // Act - a group member views the profile page.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var shown = await new MemberProfilePage(Page).GetAnswer(routes.MemberPage(memberId), label);

        // Assert - the answer is shown.
        shown.Should().Be(answer);
    }

    [Test]
    public async Task MemberPage_DoesNotShowApplicationOnlyProperty()
    {
        // Arrange - a member who answered a normal and an application-only question when joining.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var normalLabel = PropertyLabel("normal");
        var applicationOnlyLabel = PropertyLabel("application-only");
        var normalId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, normalLabel);
        var applicationOnlyId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, applicationOnlyLabel, applicationOnly: true);
        var member = await JoinChapterWithProperties(group, new Dictionary<Guid, string>
        {
            [normalId] = "Normal answer",
            [applicationOnlyId] = "Application-only answer"
        });
        var memberId = await Members.GetMemberId(member.Email);

        // Act - view the member page.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var page = new MemberProfilePage(Page);
        var normalShown = await page.GetAnswer(routes.MemberPage(memberId), normalLabel);
        var applicationOnlyShown = await page.GetAnswer(routes.MemberPage(memberId), applicationOnlyLabel);

        // Assert - the normal answer shows; the application-only one is hidden.
        normalShown.Should().Be("Normal answer");
        applicationOnlyShown.Should().BeNull();
    }

    [Test]
    public async Task MemberPage_NonMember_ReturnsNotFound()
    {
        // Arrange - a member of the chapter (the profile to view) and an outsider who isn't a member.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var propertyId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("question"));
        var target = await JoinChapterWithProperties(
            group, new Dictionary<Guid, string> { [propertyId] = "Answer" });
        var targetId = await Members.GetMemberId(target.Email);
        var outsider = await Provisioning.NewAccount("outsider");

        // Act - the outsider (not a member of this chapter) requests the member's profile page.
        await new LoginPage(Page).LogIn(outsider.Email, outsider.Password);
        var status = await new MemberProfilePage(Page).GetResponseStatus(routes.MemberPage(targetId));

        // Assert - a member's profile isn't visible to non-members: 404.
        status.Should().Be(404);
    }

    [Test]
    public async Task MemberPage_ReflectsReorderedProperties()
    {
        // Arrange - a member who answered two profile questions (created in order first, second).
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var firstLabel = PropertyLabel("first");
        var secondLabel = PropertyLabel("second");
        var firstId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, firstLabel);
        var secondId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, secondLabel);
        var member = await JoinChapterWithProperties(group, new Dictionary<Guid, string>
        {
            [firstId] = "First answer",
            [secondId] = "Second answer"
        });
        var memberId = await Members.GetMemberId(member.Email);

        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var page = new MemberProfilePage(Page);
        var memberPage = routes.MemberPage(memberId);

        // Precondition - they display in creation order.
        var before = (await page.GetLabelsInOrder(memberPage))
            .Where(x => x == firstLabel || x == secondLabel).ToArray();
        before.Should().Equal(firstLabel, secondLabel);

        // Act - the owner moves the first question down (after the second).
        await Provisioning.MoveChapterPropertyDown(owner, routes, firstId, PlatformBaseUrl);

        // Assert - the member page reflects the new order.
        var after = (await page.GetLabelsInOrder(memberPage))
            .Where(x => x == firstLabel || x == secondLabel).ToArray();
        after.Should().Equal(secondLabel, firstLabel);
    }

    [Test]
    public async Task ProfileUpdateForm_ExcludesApplicationOnlyProperty()
    {
        // Arrange - a chapter with a normal and an application-only question, and a member.
        var (owner, group) = await ProvisionOwnerChapter(GroupName());
        var routes = RoutesFor(group);
        var normalId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("normal"));
        var applicationOnlyId = await Provisioning.CreateChapterProperty(
            owner, routes, group.ChapterId, PlatformBaseUrl, PropertyLabel("application-only"), applicationOnly: true);
        var member = await JoinChapterWithProperties(
            group, new Dictionary<Guid, string> { [normalId] = "Answer" });

        // Act - the member opens their profile-update form.
        await new LoginPage(Page).LogIn(member.Email, member.Password);
        var page = new ProfileUpdatePage(Page);

        // Assert - the normal question is on the form; the application-only one is not.
        (await page.HasProperty(routes.ProfileUpdate, normalId)).Should().BeTrue();
        (await page.HasProperty(routes.ProfileUpdate, applicationOnlyId)).Should().BeFalse();
    }

    private protected abstract Task<TestAccount> JoinChapterWithProperties(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers);

    private protected abstract Task<(TestAccount Owner, TestGroup Group)> ProvisionOwnerChapter(string name);

    private protected abstract PlatformRoutes RoutesFor(TestGroup group);

    private protected abstract Task<bool> TryJoinChapterWithoutRequired(
        TestGroup group, IReadOnlyDictionary<Guid, string> answers);

    // A URL-safe, space-free group name (the DrunkenKnitwits route segment is derived from the name).
    private static string GroupName() => $"e2eprop{Guid.NewGuid():N}";

    private static string PropertyLabel(string tag) => $"E2E {tag} {Guid.NewGuid():N}";
}
