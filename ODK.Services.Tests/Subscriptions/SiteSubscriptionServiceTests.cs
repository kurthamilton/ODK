using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionServiceTests
{
    // Far enough past the one-month cooldown the tests run with to be out of it however the boundary falls.
    private static DateTime LapsedPastCooldown => DateTime.UtcNow.AddMonths(-1).AddDays(-1);

    [Test]
    public static async Task DowngradeLapsedSubscriptions_AlreadyOnTheDefaultPlan_WritesNothing()
    {
        /* Arrange - a lapsed record already naming the plan it would be moved to. Downgrading it would
           append a record saying nothing new, on every run, forever. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var defaultPlan = context.CreateSiteSubscription(free: true, isDefault: true);
        context.CreateMemberSiteSubscription(member, defaultPlan, expiresUtc: LapsedPastCooldown);

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(1);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_DefaultPlanIsNotFree_LogsAndWritesNothing()
    {
        /* Arrange - a downgrade takes no payment, so a priced target would hand the member a plan they have
           not paid for. Nothing else asserts the default plan is free. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateSiteSubscription(free: false, isDefault: true);
        context.CreateMemberSiteSubscription(member, expiresUtc: LapsedPastCooldown);

        var loggingService = new Mock<ILoggingService>();
        var service = CreateService(context, loggingService: loggingService.Object);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(1);
        loggingService.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_LapsedPastTheCooldown_MovesMemberOntoTheDefaultPlan()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var defaultPlan = context.CreateSiteSubscription(free: true, isDefault: true);
        var paidPlan = context.CreateSiteSubscription();
        var price = context.CreateSiteSubscriptionPrice(paidPlan);
        var lapsed = context.CreateMemberSiteSubscription(
            member, paidPlan, expiresUtc: LapsedPastCooldown, siteSubscriptionPrice: price, externalId: "sub_test");

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert - the paid record survives as history, and what replaces it carries nothing of the
        // purchase, an expiry above all: a free plan that expires would lapse again immediately.
        var records = RecordsFor(context, member);
        records.Should().HaveCount(2);
        lapsed.IsCurrent.Should().BeFalse();

        var current = records.Single(x => x.IsCurrent);
        current.SiteSubscriptionId.Should().Be(defaultPlan.Id);
        current.ExpiresUtc.Should().BeNull();
        current.ExternalId.Should().BeNull();
        current.PaymentId.Should().BeNull();
        current.SiteSubscriptionPriceId.Should().BeNull();
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_LapsedPastTheCooldown_NotifiesAndEmailsTheMember()
    {
        /* Arrange - nothing the member did prompts a downgrade, so they are told twice over: in the app,
           and by email, because a member who has stopped paying is not visiting the site. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateSiteSubscription(free: true, isDefault: true, name: "Free");
        context.CreateMemberSiteSubscription(member, expiresUtc: LapsedPastCooldown);

        var memberEmailService = new Mock<IMemberEmailService>();
        var service = CreateService(context, memberEmailService: memberEmailService.Object);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        var notification = context.Set<Notification>().Single(x => x.MemberId == member.Id);
        notification.Type.Should().Be(NotificationType.SubscriptionDowngraded);
        notification.Text.Should().Be("Your subscription has expired. You are now on the Free plan.");

        memberEmailService.Verify(
            x => x.SendSiteSubscriptionExpiredEmail(It.IsAny<IServiceRequest>(), member),
            Times.Once);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_NotificationsDisabled_DowngradesWithoutNotifying()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var defaultPlan = context.CreateSiteSubscription(free: true, isDefault: true);
        context.CreateMemberSiteSubscription(member, expiresUtc: LapsedPastCooldown);
        context.Create(new MemberNotificationSettings
        {
            Disabled = true,
            MemberId = member.Id,
            NotificationType = NotificationType.SubscriptionDowngraded
        });

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        context.Set<Notification>().Should().BeEmpty();
        RecordsFor(context, member).Single(x => x.IsCurrent).SiteSubscriptionId.Should().Be(defaultPlan.Id);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_PlanOnAnotherPlatform_LeavesItAlone()
    {
        /* Arrange - a deployment downgrades the plans its own platform sells. The member is told about it,
           and a Group Squirrel deployment telling a Drunken Knitwits member would write as the wrong site. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateSiteSubscription(free: true, isDefault: true);
        var otherPlatformPlan = context.CreateSiteSubscription(platform: PlatformType.DrunkenKnitwits);
        context.CreateMemberSiteSubscription(member, otherPlatformPlan, expiresUtc: LapsedPastCooldown);

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(1);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_PlatformHasNoDefaultPlan_LogsAndWritesNothing()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateMemberSiteSubscription(member, expiresUtc: LapsedPastCooldown);

        var loggingService = new Mock<ILoggingService>();
        var service = CreateService(context, loggingService: loggingService.Object);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(1);
        loggingService.Verify(x => x.Error(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_RunTwice_AppendsOneRecord()
    {
        /* Arrange - the sweep runs on a cron forever, so a member it has already moved must not collect a
           record on every run. The free plan it writes has no expiry, which is what takes them out of the
           query the second time. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateSiteSubscription(free: true, isDefault: true);
        context.CreateMemberSiteSubscription(member, expiresUtc: LapsedPastCooldown);

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(2);
        context.Set<Notification>().Should().HaveCount(1);
    }

    [Test]
    public static async Task DowngradeLapsedSubscriptions_WithinTheCooldown_WritesNothing()
    {
        /* Arrange - an expired subscription keeps what it paid for until the cooldown is up, so the stored
           expiry alone does not say whether a member should be moved. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        context.CreateSiteSubscription(free: true, isDefault: true);
        context.CreateMemberSiteSubscription(member, expiresUtc: DateTime.UtcNow.AddDays(-1));

        var service = CreateService(context);

        // Act
        await service.DowngradeLapsedSubscriptions(CreateRequest());

        // Assert
        RecordsFor(context, member).Should().HaveCount(1);
    }

    [Test]
    public static async Task StartSiteSubscriptionCheckout_PriceOnAnotherPlatform_Throws()
    {
        /* Arrange - the price id is posted by the browser, so a Drunken Knitwits plan can be submitted to a
           Group Squirrel checkout. The list it should have been chosen from is this platform's, and nothing
           downstream confines it: the payment would be booked against the plan's own platform. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var subscription = context.CreateSiteSubscription(platform: PlatformType.DrunkenKnitwits);
        var price = context.CreateSiteSubscriptionPrice(subscription);

        var service = CreateService(context);
        var request = Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.GroupSquirrel &&
            x.CurrentMember == member);

        // Act
        var act = () => service.StartSiteSubscriptionCheckout(request, price.Id, returnPath: "/");

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    private static MockOdkContext CreateMockOdkContext() => new();

    private static IServiceRequest CreateRequest() => Mock.Of<IServiceRequest>(x =>
        x.Environment == EnvironmentType.Dev &&
        x.Platform == PlatformType.GroupSquirrel);

    private static SiteSubscriptionService CreateService(
        MockOdkContext context,
        IMemberEmailService? memberEmailService = null,
        ILoggingService? loggingService = null)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        return new SiteSubscriptionService(
            unitOfWork,
            Mock.Of<IPaymentProviderFactory>(),
            Mock.Of<IPaymentService>(),
            new MemberSiteSubscriptionWriter(unitOfWork),
            new NotificationService(unitOfWork, Mock.Of<IMemberLocaleService>()),
            memberEmailService ?? Mock.Of<IMemberEmailService>(),
            loggingService ?? Mock.Of<ILoggingService>(),
            new SiteSubscriptionCooldown(months: 1));
    }

    private static MemberSiteSubscriptionRecord[] RecordsFor(MockOdkContext context, Member member)
        => context.Set<MemberSiteSubscriptionRecord>()
            .Where(x => x.MemberId == member.Id)
            .ToArray();
}
