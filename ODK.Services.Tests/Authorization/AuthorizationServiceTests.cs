using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Authorization;

namespace ODK.Services.Tests.Authorization;
public static class AuthorizationServiceTests
{
    private static readonly Guid DefaultChapterId = Guid.NewGuid();

    [Test]
    public static void GetSubscriptionStatus_MembershipDisabled_IgnoresExpiryDate()
    {
        // Arrange
        var service = CreateService();

        var subscription = CreateMemberSubscription(expiryDate: DateTime.UtcNow.AddDays(-100));
        var member = CreateMember();
        var settings = CreateChapterMembershipSettings(
            membershipDisabledAfterDaysExpired: 1,
            enabled: false);

        // Act
        var result = service.GetSubscriptionStatus(member, subscription, settings);

        // Assert
        result.Should().Be(SubscriptionStatus.Current);
    }

    [TestCase(true, ExpectedResult = SubscriptionStatus.Expired)]
    [TestCase(false, ExpectedResult = SubscriptionStatus.Current)]
    public static SubscriptionStatus GetSubscriptionStatus_MembershipEnabled_UsesExpiryDate(bool expired)
    {
        // Arrange
        var service = CreateService();

        var settings = CreateChapterMembershipSettings(
            membershipDisabledAfterDaysExpired: 100,
            enabled: true);
        var expiryDate = expired
            ? DateTime.UtcNow.AddDays(-1)
            : DateTime.UtcNow.AddDays(settings.MembershipExpiringWarningDays + 1);
        var subscription = CreateMemberSubscription(expiryDate: expiryDate);
        var member = CreateMember();

        // Act
        var result = service.GetSubscriptionStatus(member, subscription, settings);

        // Assert
        return result;
    }

    [TestCase(true, ExpectedResult = SubscriptionStatus.Expiring)]
    [TestCase(false, ExpectedResult = SubscriptionStatus.Current)]
    public static SubscriptionStatus GetSubscriptionStatus_ExpiryDateWithinWarningPeriod_ReturnsExpiring(bool expiring)
    {
        // Arrange
        var service = CreateService();

        var settings = CreateChapterMembershipSettings(membershipExpiringWarningDays: 5);
        var expiryDate = expiring
            ? DateTime.UtcNow.AddDays(1)
            : DateTime.UtcNow.AddDays(settings.MembershipExpiringWarningDays + 1);
        var subscription = CreateMemberSubscription(expiryDate: expiryDate);
        var member = CreateMember();

        // Act
        var result = service.GetSubscriptionStatus(member, subscription, settings);

        // Assert
        return result;
    }

    [TestCase(true, ExpectedResult = SubscriptionStatus.Disabled)]
    [TestCase(false, ExpectedResult = SubscriptionStatus.Expired)]
    public static SubscriptionStatus GetSubscriptionStatus_ExpiryDateOutsideExpiredPeriod_ReturnsDisabled(bool disabled)
    {
        // Arrange
        var service = CreateService();

        var settings = CreateChapterMembershipSettings();
        var expiryDate = disabled
            ? DateTime.UtcNow.AddDays(-1 * settings.MembershipDisabledAfterDaysExpired - 1)
            : DateTime.UtcNow.AddDays(-1 * settings.MembershipDisabledAfterDaysExpired + 1);
        var subscription = CreateMemberSubscription(expiryDate: expiryDate);
        var member = CreateMember();

        // Act
        var result = service.GetSubscriptionStatus(member, subscription, settings);

        // Assert
        return result;
    }

    private static ChapterMembershipSettings CreateChapterMembershipSettings(
        int membershipDisabledAfterDaysExpired = 60,
        bool enabled = true,
        int membershipExpiringWarningDays = 1)
    {
        return new ChapterMembershipSettings
        {
            ChapterId = DefaultChapterId,
            Enabled = enabled,
            MembershipDisabledAfterDaysExpired = membershipDisabledAfterDaysExpired,
            MembershipExpiringWarningDays = membershipExpiringWarningDays,
            TrialPeriodMonths = 1
        };
    }

    private static Member CreateMember()
    {
        return new Member
        {
            Activated = true
        };
    }

    private static MemberSubscription CreateMemberSubscription(
        Guid? memberId = null,
        DateTime? expiryDate = null)
    {
        return new MemberSubscription
        {
            ExpiresUtc = expiryDate,
            MemberId = memberId ?? Guid.NewGuid(),
            Type = SubscriptionType.Full
        };
    }

    private static AuthorizationService CreateService()
    {
        return new AuthorizationService();
    }
}
