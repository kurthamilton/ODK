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

    private static ChapterViewModelService CreateChapterViewModelService(MockOdkContext context)
        => new(
            MockUnitOfWorkFactory.Create(context),
            new AuthorizationService(),
            Mock.Of<ISocialMediaService>(),
            Mock.Of<ILoggingService>(),
            Mock.Of<IDistanceUnitFactory>(),
            Mock.Of<IGeolocationService>(),
            Mock.Of<ILatLongCalculator>(),
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
            .Returns(PlatformType.Default);

        return mock.Object;
    }

    private static SubscriptionsPageViewModelFactory CreateSubscriptionsPageViewModelFactory()
        => new(Mock.Of<IPaymentProviderFactory>());
}
