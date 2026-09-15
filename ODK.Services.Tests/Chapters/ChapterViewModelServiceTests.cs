using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.SocialMedia;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Chapters;

[Parallelizable]
public static class ChapterViewModelServiceTests
{
    [Test]
    public static async Task GetGroupHomePage_WhenMoveIsInsideTheBannerWindow_ReturnsIt()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = context.CreateChapter();

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow.AddDays(-29),
            PreviousPlatformName = "Meetup"
        });

        var service = CreateChapterViewModelService(context, migrationWindowDays: 30);

        // Act
        var result = await service.GetGroupHomePage(CreateChapterServiceRequest(chapter));

        // Assert
        result.RecentMove.Should().NotBeNull();
        result.RecentMove.PreviousPlatformName.Should().Be("Meetup");
    }

    [Test]
    public static async Task GetGroupHomePage_WhenMoveIsOutsideTheBannerWindow_ReturnsNull()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = context.CreateChapter();

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow.AddDays(-31),
            PreviousPlatformName = "Meetup"
        });

        var service = CreateChapterViewModelService(context, migrationWindowDays: 30);

        // Act
        var result = await service.GetGroupHomePage(CreateChapterServiceRequest(chapter));

        // Assert
        result.RecentMove.Should().BeNull();
    }

    [Test]
    public static async Task GetGroupMovedPage_WhenGroupHasNoMigration_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = context.CreateChapter();

        var service = CreateChapterViewModelService(context);

        // Act
        Func<Task> act = () => service.GetGroupMovedPage(CreateChapterServiceRequest(chapter));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task GetGroupMovedPage_WhenMovedPageNotPublished_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = context.CreateChapter();

        // Wording written but the page switched off, which is what a null MovedUtc means.
        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MessageHtml = "<p>Somewhere better</p>",
            PreviousPlatformName = "Meetup"
        });

        var service = CreateChapterViewModelService(context);

        // Act
        Func<Task> act = () => service.GetGroupMovedPage(CreateChapterServiceRequest(chapter));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task GetGroupMovedPage_WhenMovedPagePublished_ReturnsTheOrganisersWording()
    {
        // Arrange
        using var context = new MockOdkContext();

        var chapter = context.CreateChapter();

        context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MessageHtml = "<p>Somewhere better</p>",
            MovedUtc = DateTime.UtcNow,
            PreviousPlatformName = "Meetup"
        });

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupMovedPage(CreateChapterServiceRequest(chapter));

        // Assert
        result.Chapter.Id.Should().Be(chapter.Id);
        result.Migration.MessageHtml.Should().Be("<p>Somewhere better</p>");
        result.Migration.PreviousPlatformName.Should().Be("Meetup");
        result.Visitor.Should().Be(GroupMovedVisitorState.Anonymous);
    }

    [Test]
    public static async Task GetGroupMovedPage_SignedInMember_IsSentToTheGroup()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);

        CreateMigration(context, chapter);

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupMovedPage(CreateChapterServiceRequest(chapter, member));

        // Assert
        result.Visitor.Should().Be(GroupMovedVisitorState.Member);
    }

    [Test]
    public static async Task GetGroupMovedPage_SignedInWithAnInvite_IsSentToAcceptIt()
    {
        /* Arrange - the group has already said yes to them, so the page offers the invite rather than the
           join form. Someone who signed up independently after being imported lands exactly here. */
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            SentUtc = DateTime.UtcNow.AddDays(-1),
            Token = "invite-token"
        });

        CreateMigration(context, chapter);

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupMovedPage(CreateChapterServiceRequest(chapter, member));

        // Assert
        result.Visitor.Should().Be(GroupMovedVisitorState.Invited);
        result.InviteToken.Should().Be("invite-token");
    }

    [Test]
    public static async Task GetGroupMovedPage_MemberWithAStaleInvite_IsStillAMember()
    {
        /* Arrange - a group that invited somebody who then joined by another route has an invite it never
           consumed. They are in the group either way, so membership wins. */
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);

        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Token = "stale"
        });

        CreateMigration(context, chapter);

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupMovedPage(CreateChapterServiceRequest(chapter, member));

        // Assert
        result.Visitor.Should().Be(GroupMovedVisitorState.Member);
    }

    [Test]
    public static async Task GetGroupMovedPage_SignedInStranger_IsOfferedTheGroup()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        CreateMigration(context, chapter);

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupMovedPage(CreateChapterServiceRequest(chapter, member));

        // Assert
        result.Visitor.Should().Be(GroupMovedVisitorState.SignedInNotMember);
        result.InviteToken.Should().BeNull();
    }

    [Test]
    public static async Task GetGroupSubscriptionPage_ReturnsTheSameSubscriptionsAsTheChapterServicePath()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var subscription = context.CreateChapterSubscription(chapter);

        CreateCurrentSubscriptionRecord(context, chapter, member, subscription);

        var request = CreateMemberChapterServiceRequest(chapter, member);

        // Act
        var groupPage = await CreateChapterViewModelService(context).GetGroupSubscriptionPage(request);
        var chapterPage = await CreateChapterService(context).GetChapterMemberSubscriptionsViewModel(request);

        // Assert
        chapterPage.CurrentSubscription.Should().NotBeNull();
        groupPage.Subscriptions.CurrentSubscription.Should().NotBeNull();
        groupPage.Subscriptions.CurrentSubscription.Id
            .Should().Be(chapterPage.CurrentSubscription.Id);

        groupPage.Subscriptions.ChapterSubscriptions.Select(x => x.Id)
            .Should().BeEquivalentTo(chapterPage.ChapterSubscriptions.Select(x => x.Id));

        chapterPage.MemberSubscription.Should().NotBeNull();
        groupPage.Subscriptions.MemberSubscription.Should().NotBeNull();
        groupPage.Subscriptions.MemberSubscription.ExpiresUtc
            .Should().Be(chapterPage.MemberSubscription.ExpiresUtc);
    }

    [Test]
    public static async Task GetGroupSubscriptionPage_WhenCurrentTierDisabled_StillReturnsItAsTheCurrentSubscription()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);

        var subscription = context.CreateChapterSubscription(chapter);
        subscription.Disabled = true;

        CreateCurrentSubscriptionRecord(context, chapter, member, subscription);

        var service = CreateChapterViewModelService(context);

        // Act
        var result = await service.GetGroupSubscriptionPage(
            CreateMemberChapterServiceRequest(chapter, member));

        // Assert
        result.Subscriptions.CurrentSubscription.Should().NotBeNull();
        result.Subscriptions.CurrentSubscription.Id.Should().Be(subscription.Id);
        result.Subscriptions.ChapterSubscriptions.Should().BeEmpty();
    }

    [Test]
    public static async Task GetGroupSubscriptionPage_WhenMemberNotInChapter_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        var service = CreateChapterViewModelService(context);

        // Act
        Func<Task> act = () => service.GetGroupSubscriptionPage(
            CreateMemberChapterServiceRequest(chapter, member));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    private static ChapterService CreateChapterService(MockOdkContext context)
        => new(MockUnitOfWorkFactory.Create(context), CreateSubscriptionsPageViewModelFactory());

    private static ChapterMigration CreateMigration(MockOdkContext context, Chapter chapter)
        => context.Create(new ChapterMigration
        {
            ChapterId = chapter.Id,
            MovedUtc = DateTime.UtcNow
        });

    private static IChapterServiceRequest CreateChapterServiceRequest(
        Chapter chapter, Member? currentMember = null)
    {
        var mock = new Mock<IChapterServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter);

        mock.Setup(x => x.CurrentMemberOrDefault)
            .Returns(currentMember);

        mock.Setup(x => x.Environment)
            .Returns(EnvironmentType.Dev);

        mock.Setup(x => x.Platform)
            .Returns(PlatformType.GroupSquirrel);

        return mock.Object;
    }

    private static ChapterViewModelService CreateChapterViewModelService(
        MockOdkContext context, int migrationWindowDays = 0)
        => new(
            MockUnitOfWorkFactory.Create(context),
            new AuthorizationService(),
            Mock.Of<ISocialMediaService>(),
            Mock.Of<ILoggingService>(),
            Mock.Of<IDistanceUnitFactory>(),
            Mock.Of<IGeolocationService>(),
            Mock.Of<ILatLongCalculator>(),
            new ChapterViewModelServiceSettings { MigrationWindowDays = migrationWindowDays },
            new SiteSubscriptionCooldown(months: 0),
            CreateSubscriptionsPageViewModelFactory());

    /// <remarks>
    /// No <see cref="MemberSubscriptionRecord.ExternalId"/>, so the payment provider is never reached and
    /// the mocked factory never has to return one.
    /// </remarks>
    private static MemberSubscriptionRecord CreateCurrentSubscriptionRecord(
        MockOdkContext context,
        Chapter chapter,
        Member member,
        ChapterSubscription subscription)
        => context.Create(new MemberSubscriptionRecord
        {
            ChapterId = chapter.Id,
            ChapterSubscriptionId = subscription.Id,
            ExpiresUtc = DateTime.UtcNow.AddMonths(1),
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            Months = 12,
            PurchasedUtc = DateTime.UtcNow,
            Type = SubscriptionType.Full
        });

    private static IMemberChapterServiceRequest CreateMemberChapterServiceRequest(
        Chapter chapter,
        Member currentMember)
    {
        var mock = new Mock<IMemberChapterServiceRequest>();

        mock.Setup(x => x.Chapter)
            .Returns(chapter);

        mock.Setup(x => x.CurrentMember)
            .Returns(currentMember);

        mock.Setup(x => x.CurrentMemberOrDefault)
            .Returns(currentMember);

        // Chapter subscriptions are stored per environment, and the mock context creates them as Dev.
        mock.Setup(x => x.Environment)
            .Returns(EnvironmentType.Dev);

        mock.Setup(x => x.Platform)
            .Returns(PlatformType.GroupSquirrel);

        return mock.Object;
    }

    private static SubscriptionsPageViewModelFactory CreateSubscriptionsPageViewModelFactory()
        => new(Mock.Of<IPaymentProviderFactory>());
}
