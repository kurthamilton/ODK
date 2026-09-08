using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Data;

/// <summary>
/// How a lapsed site subscription is found. The cooldown decides which side of the line a stored expiry
/// falls on, and the platform and environment come from the plan rather than from the member, so a
/// deployment sweeps its own plans and nobody else's.
/// </summary>
[Parallelizable]
public static class MemberSiteSubscriptionRecordQueryBuilderTests
{
    [Test]
    public static async Task ActiveAndExpired_EveryShapeOfExpiry_PartitionThemBetweenThem()
    {
        /* Arrange - one record of every shape an expiry takes, either side of a one-month cooldown. The two
           filters are complements read by different callers, so a record neither returns is one nothing can
           act on. */
        using var context = CreateMockOdkContext();

        var cooldown = new SiteSubscriptionCooldown(months: 1);
        var utcNow = DateTime.UtcNow;

        var free = context.CreateMemberSiteSubscription(
            context.CreateMember(), expiresUtc: null);
        var live = context.CreateMemberSiteSubscription(
            context.CreateMember(), expiresUtc: utcNow.AddDays(30));
        var withinCooldown = context.CreateMemberSiteSubscription(
            context.CreateMember(), expiresUtc: utcNow.AddDays(-1));
        var lapsed = context.CreateMemberSiteSubscription(
            context.CreateMember(), expiresUtc: utcNow.AddMonths(-1).AddDays(-1));

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var active = await unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .Active(cooldown)
            .GetAll()
            .Run();

        var expired = await unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .Expired(cooldown)
            .GetAll()
            .Run();

        // Assert
        active.Select(x => x.Id).Should().BeEquivalentTo(new[] { free.Id, live.Id, withinCooldown.Id });
        expired.Select(x => x.Id).Should().Equal(lapsed.Id);
    }

    [Test]
    public static async Task ForEnvironment_PlanInAnotherEnvironment_IsExcluded()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var dev = context.CreateMemberSiteSubscription(
            context.CreateMember(),
            context.CreateSiteSubscription(environment: EnvironmentType.Dev));
        context.CreateMemberSiteSubscription(
            context.CreateMember(),
            context.CreateSiteSubscription(environment: EnvironmentType.Prod));

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .ForEnvironment(EnvironmentType.Dev)
            .GetAll()
            .Run();

        // Assert
        result.Select(x => x.Id).Should().Equal(dev.Id);
    }

    [Test]
    public static async Task ForPlatform_PlanOnAnotherPlatform_IsExcluded()
    {
        /* Arrange - the platform is the plan's, not the member's: a plan belongs to the platform that sells
           it, whichever site the member signed up on. */
        using var context = CreateMockOdkContext();

        var groupSquirrel = context.CreateMemberSiteSubscription(
            context.CreateMember(),
            context.CreateSiteSubscription(platform: PlatformType.Default));
        context.CreateMemberSiteSubscription(
            context.CreateMember(),
            context.CreateSiteSubscription(platform: PlatformType.DrunkenKnitwits));

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .ForPlatform(PlatformType.Default)
            .GetAll()
            .Run();

        // Assert
        result.Select(x => x.Id).Should().Equal(groupSquirrel.Id);
    }

    [Test]
    public static async Task MostRecent_MemberWithSeveralRecords_ReturnsOnlyTheNewest()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var utcNow = DateTime.UtcNow;

        context.CreateMemberSiteSubscription(member, createdUtc: utcNow.AddYears(-2));
        var newest = context.CreateMemberSiteSubscription(member, createdUtc: utcNow.AddYears(-1));

        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        var result = await unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .ForMember(member.Id)
            .MostRecent()
            .GetSingleOrDefault()
            .Run();

        // Assert
        result?.Id.Should().Be(newest.Id);
    }

    private static MockOdkContext CreateMockOdkContext() => new();
}
