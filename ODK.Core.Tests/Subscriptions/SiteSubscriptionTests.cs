using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Subscriptions;

namespace ODK.Core.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionTests
{
    [TestCase(null, 0)]
    [TestCase(null, 100)]
    [TestCase(5, 0)]
    [TestCase(5, 4)]
    [TestCase(5, 5)]
    [TestCase(5, 6)]
    public static void HasCapacity_AgreesWithRemainingCapacity(int? memberLimit, int memberCount)
    {
        // Arrange - two readings of one rule, so they must never disagree.
        var subscription = new SiteSubscription { MemberLimit = memberLimit };

        // Act
        var hasCapacity = subscription.HasCapacity(memberCount);
        var remaining = subscription.RemainingCapacity(memberCount);

        // Assert
        hasCapacity.Should().Be(remaining is null or > 0);
    }

    [Test]
    public static void IsActive_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = false, Free = true };

        // Act
        var result = subscription.IsActive([]);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsActive_WhenFreeAndPriceless_ReturnsTrue()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = true };

        // Act
        var result = subscription.IsActive([]);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenPricedAndNotFree_ReturnsTrue()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = false };

        // Act
        var result = subscription.IsActive([new SiteSubscriptionPrice()]);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenNeitherFreeNorPriced_ReturnsFalse()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = false };

        // Act
        var result = subscription.IsActive([]);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void RemainingCapacity_NoLimit_ReturnsNull()
    {
        // Arrange - a plan stating no limit permits any number.
        var subscription = new SiteSubscription { MemberLimit = null };

        // Act
        var result = subscription.RemainingCapacity(100);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void RemainingCapacity_LimitNotReached_ReturnsWhatIsLeft()
    {
        // Arrange
        var subscription = new SiteSubscription { MemberLimit = 10 };

        // Act
        var result = subscription.RemainingCapacity(4);

        // Assert
        result.Should().Be(6);
    }

    [Test]
    public static void RemainingCapacity_LimitReached_ReturnsZero()
    {
        // Arrange
        var subscription = new SiteSubscription { MemberLimit = 10 };

        // Act
        var result = subscription.RemainingCapacity(10);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public static void RemainingCapacity_MoreMembersThanTheLimit_ReturnsZero()
    {
        // Arrange - a plan downgraded under a group that is already fuller than the new limit.
        var subscription = new SiteSubscription { MemberLimit = 10 };

        // Act
        var result = subscription.RemainingCapacity(25);

        // Assert
        result.Should().Be(0);
    }
}
