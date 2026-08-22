using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Subscriptions;

namespace ODK.Core.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionExtensionsTests
{
    [Test]
    public static void GroupLimitOrDefault_NoSubscription_ReturnsDefaultLimit()
    {
        // Arrange
        SiteSubscription? subscription = null;

        // Act
        var result = subscription.GroupLimitOrDefault();

        // Assert
        result.Should().Be(SiteSubscription.DefaultGroupLimit);
    }

    [Test]
    public static void GroupLimitOrDefault_SubscriptionWithLimit_ReturnsSubscriptionLimit()
    {
        // Arrange
        var subscription = new SiteSubscription { GroupLimit = 3 };

        // Act
        var result = subscription.GroupLimitOrDefault();

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public static void GroupLimitOrDefault_SubscriptionWithNoLimit_ReturnsNull()
    {
        // Arrange
        var subscription = new SiteSubscription { GroupLimit = null };

        // Act
        var result = subscription.GroupLimitOrDefault();

        // Assert
        result.Should().BeNull();
    }
}
