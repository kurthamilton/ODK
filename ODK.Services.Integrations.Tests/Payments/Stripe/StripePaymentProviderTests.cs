using FluentAssertions;
using Moq;
using ODK.Core.Payments;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;
using Stripe;

namespace ODK.Services.Integrations.Tests.Payments.Stripe;

[Parallelizable]
public static class StripePaymentProviderTests
{
    [Test]
    public static async Task MapSubscription_PopulatesDatesAndPlanFromItem()
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
        var result = await CreateProvider().MapSubscription(subscription);

        // Assert
        result.Should().NotBeNull();
        result!.NextBillingDate.Should().Be(periodEnd);
        result.LastPaymentDate.Should().Be(periodStart);
        result.ExternalSubscriptionPlanId.Should().Be("price_123");
        result.Status.Should().Be(ExternalSubscriptionStatus.Active);
    }

    [Test]
    public static async Task MapSubscription_NoItems_ReturnsNullAndLogsError()
    {
        // Arrange - a subscription with no item cannot carry the plan or billing dates, so it is useless.
        var loggingService = new Mock<ILoggingService>();

        var subscription = new Subscription
        {
            Id = "sub_123",
            Status = "active",
            Metadata = new Dictionary<string, string>(),
            Items = new StripeList<SubscriptionItem> { Data = [] }
        };

        // Act
        var result = await CreateProvider(loggingService.Object).MapSubscription(subscription);

        // Assert
        result.Should().BeNull();
        loggingService.Verify(x => x.Error(It.Is<string>(m => m.Contains("sub_123"))), Times.Once);
    }

    [Test]
    public static async Task MapSubscription_CancelAtSet_StatusCancelled()
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
        var result = await CreateProvider().MapSubscription(subscription);

        // Assert
        result!.Status.Should().Be(ExternalSubscriptionStatus.Cancelled);
        result.CancelDate.Should().Be(subscription.CancelAt);
    }

    private static StripePaymentProvider CreateProvider(ILoggingService? loggingService = null)
        => new StripePaymentProvider(
            new SitePaymentSettings { ApiSecretKey = "sk_test_dummy" },
            loggingService ?? new Mock<ILoggingService>().Object,
            connectedAccountId: null,
            new StripePaymentProviderSettings
            {
                ConnectedAccountBaseUrl = string.Empty,
                ConnectedAccountCommissionPercentage = 0,
                ConnectedAccountMcc = string.Empty,
                ConnectedAccountProductDescription = string.Empty
            });
}
