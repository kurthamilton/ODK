using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Subscriptions;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionAdminServiceTests
{
    [Test]
    public static async Task AddSiteSubscriptionPrice_FreeSubscription_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var currency = context.CreateCurrency();
        var subscription = context.CreateSiteSubscription(free: true);
        var service = CreateService(context);

        // Act
        var result = await service.AddSiteSubscriptionPrice(
            SiteAdminRequest(context),
            subscription.Id,
            new SiteSubscriptionPriceCreateModel
            {
                Amount = 5,
                CurrencyId = currency.Id,
                Frequency = SiteSubscriptionFrequency.Monthly
            });

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscriptionPrice>().Should().BeEmpty();
    }

    [Test]
    public static async Task AddSiteSubscriptionPrice_PaidSubscription_AddsThePrice()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var currency = context.CreateCurrency();
        var subscription = context.CreateSiteSubscription();
        var service = CreateService(context);

        // Act
        var result = await service.AddSiteSubscriptionPrice(
            SiteAdminRequest(context),
            subscription.Id,
            new SiteSubscriptionPriceCreateModel
            {
                Amount = 5,
                CurrencyId = currency.Id,
                Frequency = SiteSubscriptionFrequency.Monthly
            });

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SiteSubscriptionPrice>()
            .Single(x => x.SiteSubscriptionId == subscription.Id).Amount.Should().Be(5);
    }

    [Test]
    public static async Task MakeDefault_FreeSubscription_MakesItTheDefault()
    {
        // Arrange - a free subscription needs no price to be the one every new account lands on.
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription(free: true);
        var service = CreateService(context);

        // Act
        var result = await service.MakeDefault(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SiteSubscription>().Single(x => x.Id == subscription.Id).Default.Should().BeTrue();
    }

    [Test]
    public static async Task MakeDefault_NeitherFreeNorPriced_Fails()
    {
        // Arrange - nobody can be put on this subscription, so it cannot be what sign-ups default to.
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var service = CreateService(context);

        // Act
        var result = await service.MakeDefault(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Single(x => x.Id == subscription.Id).Default.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateSiteSubscription_FreeWithPaidPrice_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateSiteSubscriptionPrice(subscription, amount: 5);
        var service = CreateService(context);

        // Act
        var result = await service.UpdateSiteSubscription(
            SiteAdminRequest(context), subscription.Id, CreateModel(free: true));

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Single(x => x.Id == subscription.Id).Free.Should().BeFalse();
    }

    [Test]
    public static async Task UpdateSiteSubscription_FreeWithZeroPrice_FlagsItFree()
    {
        /* Arrange - a zero-amount price is how a free subscription was expressed before the flag, so
           flagging one must not require deleting the price first. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateSiteSubscriptionPrice(subscription, amount: 0);
        var service = CreateService(context);

        // Act
        var result = await service.UpdateSiteSubscription(
            SiteAdminRequest(context), subscription.Id, CreateModel(free: true));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SiteSubscription>().Single(x => x.Id == subscription.Id).Free.Should().BeTrue();
    }

    private static SiteSubscriptionCreateModel CreateModel(bool free) => new()
    {
        Description = "Description",
        Enabled = true,
        FallbackSiteSubscriptionId = null,
        Features = [],
        Free = free,
        GroupLimit = 1,
        MemberLimit = null,
        Name = "Test Subscription",
        SitePaymentSettingId = Guid.NewGuid()
    };

    private static MockOdkContext CreateMockOdkContext() => new();

    private static SiteSubscriptionAdminService CreateService(MockOdkContext context) => new(
        MockUnitOfWorkFactory.Create(context),
        CreateHtmlValidator(),
        Mock.Of<IPaymentProviderFactory>(),
        new SiteSubscriptionCooldown(months: 0));

    // Set up explicitly rather than left bare: a Mock.Of with no configured return hands back null from
    // Validate, which reads as a pass.
    private static IHtmlValidator CreateHtmlValidator() => Mock.Of<IHtmlValidator>(x =>
        x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()) == ServiceResult.Successful());

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}
