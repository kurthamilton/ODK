using FluentAssertions;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Payments;
using Stripe;

namespace ODK.Services.Integrations.Tests.Payments.Stripe;

[Parallelizable]
public static class StripePaymentProviderTests
{
    [Test]
    public static void MapSubscription_PopulatesDatesAndPlanFromItem()
    {
        // Arrange
        var periodStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var subscription = new Subscription
        {
            Id = "sub_123",
            Status = "active",
            Metadata = new Dictionary<string, string>(),
            Items = new StripeList<SubscriptionItem>
            {
                Data =
                [
                    new SubscriptionItem
                    {
                        CurrentPeriodStart = periodStart,
                        CurrentPeriodEnd = periodEnd,
                        Price = new Price { Id = "price_123" }
                    }
                ]
            }
        };

        // Act
        var result = StripePaymentProvider.MapSubscription(subscription);

        // Assert
        result.Should().NotBeNull();
        result!.NextBillingDate.Should().Be(periodEnd);
        result.LastPaymentDate.Should().Be(periodStart);
        result.ExternalSubscriptionPlanId.Should().Be("price_123");
        result.Status.Should().Be(ExternalSubscriptionStatus.Active);
    }

    [Test]
    public static void MapSubscription_NoItems_ReturnsNull()
    {
        // Arrange - a subscription with no item cannot carry the plan or billing dates, so it is useless.
        var subscription = new Subscription
        {
            Id = "sub_123",
            Status = "active",
            Metadata = new Dictionary<string, string>(),
            Items = new StripeList<SubscriptionItem> { Data = [] }
        };

        // Act
        var result = StripePaymentProvider.MapSubscription(subscription);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void MapSubscription_CancelAtSet_StatusCancelled()
    {
        // Arrange
        var subscription = new Subscription
        {
            Id = "sub_123",
            Status = "active",
            CancelAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Metadata = new Dictionary<string, string>(),
            Items = new StripeList<SubscriptionItem>
            {
                Data = [new SubscriptionItem { Price = new Price { Id = "price_123" } }]
            }
        };

        // Act
        var result = StripePaymentProvider.MapSubscription(subscription);

        // Assert
        result!.Status.Should().Be(ExternalSubscriptionStatus.Cancelled);
        result.CancelDate.Should().Be(subscription.CancelAt);
    }
}
