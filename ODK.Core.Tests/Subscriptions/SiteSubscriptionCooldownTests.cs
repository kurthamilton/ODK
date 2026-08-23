using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Subscriptions;

namespace ODK.Core.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionCooldownTests
{
    [Test]
    public static void ActiveAfterUtc_ReturnsNowLessTheCooldown()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 2);
        var utcNow = new DateTime(2026, 3, 15, 9, 30, 0, DateTimeKind.Utc);

        // Act
        var result = cooldown.ActiveAfterUtc(utcNow);

        // Assert
        result.Should().Be(new DateTime(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc));
    }

    [Test]
    public static void ActiveAfterUtc_WhenMonthsNegative_ReturnsNow()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: -3);
        var utcNow = new DateTime(2026, 3, 15, 9, 30, 0, DateTimeKind.Utc);

        // Act
        var result = cooldown.ActiveAfterUtc(utcNow);

        // Assert
        result.Should().Be(utcNow);
    }

    [Test]
    public static void IsActive_WhenExpiredBeyondCooldown_ReturnsFalse()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 1);
        var utcNow = DateTime.UtcNow;

        // Act
        var result = cooldown.IsActive(utcNow.AddMonths(-1).AddDays(-1), utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsActive_WhenExpiredWithinCooldown_ReturnsTrue()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 1);
        var utcNow = DateTime.UtcNow;

        // Act
        var result = cooldown.IsActive(utcNow.AddDays(-1), utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenExpiredWithNoCooldown_ReturnsFalse()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 0);
        var utcNow = DateTime.UtcNow;

        // Act
        var result = cooldown.IsActive(utcNow.AddSeconds(-1), utcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsActive_WhenExpiryNull_ReturnsTrue()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 0);

        // Act
        var result = cooldown.IsActive(expiresUtc: null, DateTime.UtcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenNotYetExpired_ReturnsTrue()
    {
        // Arrange
        var cooldown = new SiteSubscriptionCooldown(months: 0);
        var utcNow = DateTime.UtcNow;

        // Act
        var result = cooldown.IsActive(utcNow.AddDays(1), utcNow);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void Months_WhenNegative_IsNone()
    {
        // Act
        var cooldown = new SiteSubscriptionCooldown(months: -1);

        // Assert
        cooldown.Months.Should().Be(0);
    }
}
