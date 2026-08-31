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
    public static void MapSettlement_BalanceTransactionNotSettled_FeeAndNetNull()
    {
        // Arrange - a charge Stripe has not settled has no balance transaction to read a fee off
        var charge = new Charge
        {
            Id = "ch_123",
            Amount = 2500,
            Currency = "gbp"
        };

        // Act
        var result = StripePaymentProvider.MapSettlement(charge);

        // Assert - null rather than zero: nothing is known yet, which is not the same as costing nothing
        result.Amount.Should().Be(25m);
        result.FeeAmount.Should().BeNull();
        result.NetAmount.Should().BeNull();
        result.SettlementCurrencyCode.Should().BeNull();
        result.Complete.Should().BeFalse();
    }

    [Test]
    public static void MapSettlement_SettledCharge_ReadsWhatTheChargeLeftUsWith()
    {
        // Arrange
        var charge = CreateSettledCharge();

        // Act
        var result = StripePaymentProvider.MapSettlement(charge);

        /* Assert - the charge is collected whole, so the net is the amount less Stripe's fee and nothing
           here says what the group will get; that split is made when the transfer is. */
        result.Amount.Should().Be(25m);
        result.ChargeId.Should().Be("ch_123");
        result.CurrencyCode.Should().Be("gbp");
        result.FeeAmount.Should().Be(0.45m);
        result.NetAmount.Should().Be(24.55m);
        result.Complete.Should().BeTrue();
    }

    [Test]
    public static void MapSettlement_ProviderMadeItsOwnTransfer_CarriesTheTransfer()
    {
        /* Arrange - a destination charge, which Stripe split and transferred itself. Its transfer is the
           only handle a refund of that payment has for taking the group's share back. */
        var transferredUtc = new DateTime(2026, 5, 7, 16, 30, 0, DateTimeKind.Utc);

        var charge = CreateSettledCharge();
        charge.ApplicationFeeAmount = 250;
        charge.Transfer = new Transfer
        {
            Id = "tr_123",
            Created = transferredUtc
        };

        // Act
        var result = StripePaymentProvider.MapSettlement(charge);

        // Assert
        result.CollectedCommissionAmount.Should().Be(2.50m);
        result.TransferId.Should().Be("tr_123");
        result.TransferredUtc.Should().Be(transferredUtc);
    }

    [Test]
    public static void MapSettlement_ChargeCollectedWhole_HasNoTransfer()
    {
        // Arrange - collected whole, so the transfer is ours to make and ours to record
        var charge = CreateSettledCharge();

        // Act
        var result = StripePaymentProvider.MapSettlement(charge);

        // Assert
        result.CollectedCommissionAmount.Should().BeNull();
        result.TransferId.Should().BeNull();
        result.TransferredUtc.Should().BeNull();
    }

    [Test]
    public static void MapSettlement_SettlementCurrencyDiffersFromCharge_CarriesSettlementCurrency()
    {
        // Arrange - a euro charge settling into a sterling balance; the fee and net are in sterling
        var charge = CreateSettledCharge();
        charge.Currency = "eur";
        charge.BalanceTransaction!.Currency = "gbp";

        // Act
        var result = StripePaymentProvider.MapSettlement(charge);

        // Assert - the code travels with the amounts it applies to, so nothing can render them as euros
        result.CurrencyCode.Should().Be("eur");
        result.SettlementCurrencyCode.Should().Be("gbp");
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
        string? connectedAccountBusinessName = null,
        PlatformType platform = PlatformType.Default)
        => new StripePaymentProvider(
            loggingService ?? new Mock<ILoggingService>().Object,
            new StripePaymentProviderSettings
            {
                ConnectedAccountBusinessName = connectedAccountBusinessName ?? "{platform.title} - {group.name}",
                ConnectedAccountCommissionPercentage = 0,
                ConnectedAccountMcc = string.Empty,
                ConnectedAccountProductDescription = string.Empty,
                Platforms = new Dictionary<PlatformType, StripePaymentProviderPlatformSettings>
                {
                    [platform] = new StripePaymentProviderPlatformSettings
                    {
                        ConnectedAccountBaseUrl = string.Empty,
                        PublicApiKey = string.Empty,
                        SecretApiKey = "sk_test_dummy"
                    }
                },
                SettlementReadDelay = TimeSpan.Zero
            },
            platformProvider ?? CreateMockPlatformProvider("Platform").Object,
            platform);

    // £25.00 charged, 45p taken by Stripe, £24.55 landing in our balance.
    private static Charge CreateSettledCharge() => new Charge
    {
        Id = "ch_123",
        Amount = 2500,
        Currency = "gbp",
        BalanceTransaction = new BalanceTransaction
        {
            Amount = 2500,
            Currency = "gbp",
            Fee = 45,
            Net = 2455
        }
    };
}
