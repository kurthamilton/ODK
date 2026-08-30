using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Subscriptions;

namespace ODK.Core.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionTests
{
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
}
