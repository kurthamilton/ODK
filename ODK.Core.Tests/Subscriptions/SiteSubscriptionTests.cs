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
        var result = subscription.IsActive([], paymentsEnabled: true);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsActive_WhenFreeAndPriceless_ReturnsTrue()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = true };

        // Act
        var result = subscription.IsActive([], paymentsEnabled: true);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenFreeAndPaymentSettingsDisabled_ReturnsTrue()
    {
        // Arrange - a free subscription takes no payment, so the provider being off does not affect it.
        var subscription = new SiteSubscription { Enabled = true, Free = true };

        // Act
        var result = subscription.IsActive([], paymentsEnabled: false);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenPricedAndNotFree_ReturnsTrue()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = false };

        // Act
        var result = subscription.IsActive([new SiteSubscriptionPrice()], paymentsEnabled: true);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsActive_WhenPricedAndPaymentSettingsDisabled_ReturnsFalse()
    {
        // Arrange - nothing can be bought while the provider is off, and there is no free route in.
        var subscription = new SiteSubscription { Enabled = true, Free = false };

        // Act
        var result = subscription.IsActive([new SiteSubscriptionPrice()], paymentsEnabled: false);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsActive_WhenNeitherFreeNorPriced_ReturnsFalse()
    {
        // Arrange
        var subscription = new SiteSubscription { Enabled = true, Free = false };

        // Act
        var result = subscription.IsActive([], paymentsEnabled: true);

        // Assert
        result.Should().BeFalse();
    }
}
