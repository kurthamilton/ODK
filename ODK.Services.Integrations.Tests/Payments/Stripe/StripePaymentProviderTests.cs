using FluentAssertions;
using Moq;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Platforms;
using Stripe;

namespace ODK.Services.Integrations.Tests.Payments.Stripe;

[Parallelizable]
public static class StripePaymentProviderTests
{
    [Test]
    public static void GetConnectedAccountBusinessName_DefaultPlatformChapter_InterpolatesPlatformAndGroupName()
    {
        // Arrange
        var chapter = CreateChapter(PlatformType.Default, "Bristol Board Games");

        var provider = CreateProvider(
            platformProvider: CreateMockPlatformProvider("Group Squirrel").Object);

        // Act
        var result = provider.GetConnectedAccountBusinessName(chapter);

        // Assert
        result.Should().Be("Group Squirrel - Bristol Board Games");
    }

    [Test]
    public static void GetConnectedAccountBusinessName_DrunkenKnitwitsChapter_UsesChapterFullName()
    {
        // Arrange
        var chapter = CreateChapter(PlatformType.DrunkenKnitwits, "Bristol");

        var provider = CreateProvider(
            platformProvider: CreateMockPlatformProvider("Group Squirrel").Object);

        // Act
        var result = provider.GetConnectedAccountBusinessName(chapter);

        // Assert
        result.Should().Be("Bristol Drunken Knitwits");
    }

    [Test]
    public static void GetConnectedAccountBusinessName_NamesTheChaptersPlatform()
    {
        // Arrange
        var chapter = CreateChapter(PlatformType.Default, "Bristol Board Games");

        var platformProvider = CreateMockPlatformProvider("Group Squirrel");

        // Act
        CreateProvider(platformProvider: platformProvider.Object)
            .GetConnectedAccountBusinessName(chapter);

        // Assert - the group's platform, never the platform serving the request
        platformProvider.Verify(x => x.GetName(PlatformType.Default), Times.Once);
    }

    [Test]
    public static void GetConnectedAccountBusinessName_UsesConfiguredTemplate()
    {
        // Arrange
        var chapter = CreateChapter(PlatformType.Default, "Bristol Board Games");

        var provider = CreateProvider(
            platformProvider: CreateMockPlatformProvider("Group Squirrel").Object,
            connectedAccountBusinessName: "{group.name} on {platform.title}");

        // Act
        var result = provider.GetConnectedAccountBusinessName(chapter);

        // Assert
        result.Should().Be("Bristol Board Games on Group Squirrel");
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
        result.Should().NotBeNull();
        result.Status.Should().Be(ExternalSubscriptionStatus.Cancelled);
        result.CancelDate.Should().Be(subscription.CancelAt);
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
        result.NextBillingDate.Should().Be(periodEnd);
        result.LastPaymentDate.Should().Be(periodStart);
        result.ExternalSubscriptionPlanId.Should().Be("price_123");
        result.Status.Should().Be(ExternalSubscriptionStatus.Active);
    }

    private static Chapter CreateChapter(PlatformType platform, string name) => new Chapter
    {
        Name = name,
        Platform = platform,
        Slug = name.Replace(" ", "-").ToLowerInvariant()
    };

    private static Mock<IPlatformProvider> CreateMockPlatformProvider(string name)
    {
        var mock = new Mock<IPlatformProvider>();

        mock.Setup(x => x.GetName(It.IsAny<PlatformType>()))
            .Returns(name);

        return mock;
    }

    private static StripePaymentProvider CreateProvider(
        ILoggingService? loggingService = null,
        IPlatformProvider? platformProvider = null,
        string? connectedAccountBusinessName = null)
        => new StripePaymentProvider(
            new SitePaymentSettings { ApiSecretKey = "sk_test_dummy" },
            loggingService ?? new Mock<ILoggingService>().Object,
            connectedAccountId: null,
            new StripePaymentProviderSettings
            {
                ConnectedAccountBaseUrl = string.Empty,
                ConnectedAccountBusinessName = connectedAccountBusinessName ?? "{platform.title} - {group.name}",
                ConnectedAccountCommissionPercentage = 0,
                ConnectedAccountMcc = string.Empty,
                ConnectedAccountProductDescription = string.Empty
            },
            platformProvider ?? CreateMockPlatformProvider("Platform").Object);
}
