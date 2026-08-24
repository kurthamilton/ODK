using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Platforms;
using ODK.Services.Subscriptions;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionAdminServiceTests
{
    private const string PlatformName = "Test";

    [Test]
    public static async Task AddSiteSubscription_ExistingProduct_ReusesIt()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var paymentSettings = context.CreateSitePaymentSettings();
        var product = context.CreateSitePaymentProduct(paymentSettings, externalId: "existing-product");
        var paymentProvider = CreatePaymentProvider(createdProductId: "new-product");
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.AddSiteSubscription(
            SiteAdminRequest(context), CreateModel(free: false, paymentSettings.Id));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SitePaymentProduct>().Should().HaveCount(1);
        paymentProvider.Verify(x => x.CreateProduct(It.IsAny<string>()), Times.Never);

        context.Set<SiteSubscription>()
            .Single(x => x.Id == result.Value).SitePaymentProductId.Should().Be(product.Id);
    }

    [Test]
    public static async Task AddSiteSubscription_NoExistingProduct_CreatesOneNamedForThePlatform()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var paymentSettings = context.CreateSitePaymentSettings();
        var paymentProvider = CreatePaymentProvider(createdProductId: "new-product");
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.AddSiteSubscription(
            SiteAdminRequest(context), CreateModel(free: false, paymentSettings.Id));

        // Assert
        result.Success.Should().BeTrue();
        paymentProvider.Verify(x => x.CreateProduct($"{PlatformName} Platform"), Times.Once);

        var product = context.Set<SitePaymentProduct>().Single();
        product.ExternalId.Should().Be("new-product");
        product.Platform.Should().Be(PlatformType.Default);
        product.SitePaymentSettingId.Should().Be(paymentSettings.Id);

        context.Set<SiteSubscription>()
            .Single(x => x.Id == result.Value).SitePaymentProductId.Should().Be(product.Id);
    }

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
        var service = CreateService(
            context, CreatePaymentProviderFactory(CreatePaymentProvider(createdProductId: null).Object));

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
    public static async Task AddSiteSubscriptionPrice_PaidSubscription_CreatesThePlanUnderTheProduct()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var currency = context.CreateCurrency();
        var paymentSettings = context.CreateSitePaymentSettings();
        var product = context.CreateSitePaymentProduct(paymentSettings, externalId: "platform-product");
        var subscription = context.CreateSiteSubscription(
            sitePaymentProduct: product, sitePaymentSettings: paymentSettings);
        var paymentProvider = CreatePaymentProvider(createdProductId: null);
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

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
        paymentProvider.Verify(
            x => x.CreateSubscriptionPlan(
                It.Is<ExternalSubscriptionPlan>(p => p.ExternalProductId == product.ExternalId)),
            Times.Once);
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

    private static SiteSubscriptionCreateModel CreateModel(
        bool free, Guid? sitePaymentSettingId = null) => new()
    {
        Description = "Description",
        Enabled = true,
        FallbackSiteSubscriptionId = null,
        Features = [],
        Free = free,
        GroupLimit = 1,
        MemberLimit = null,
        Name = "Test Subscription",
        SitePaymentSettingId = sitePaymentSettingId ?? Guid.NewGuid()
    };

    private static MockOdkContext CreateMockOdkContext() => new();

    private static Mock<IPaymentProvider> CreatePaymentProvider(string? createdProductId)
    {
        var paymentProvider = new Mock<IPaymentProvider>();

        paymentProvider
            .Setup(x => x.ActivateSubscriptionPlan(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Successful());

        paymentProvider
            .Setup(x => x.CreateProduct(It.IsAny<string>()))
            .ReturnsAsync(createdProductId);

        paymentProvider
            .Setup(x => x.CreateSubscriptionPlan(It.IsAny<ExternalSubscriptionPlan>()))
            .ReturnsAsync("plan-external-id");

        return paymentProvider;
    }

    private static IPaymentProviderFactory CreatePaymentProviderFactory(IPaymentProvider paymentProvider)
    {
        var factory = new Mock<IPaymentProviderFactory>();

        factory
            .Setup(x => x.GetSitePaymentProvider(It.IsAny<SitePaymentSettings>()))
            .Returns(paymentProvider);

        factory
            .Setup(x => x.GetSitePaymentProvider(
                It.IsAny<IReadOnlyCollection<SitePaymentSettings>>(), It.IsAny<Guid?>()))
            .Returns(paymentProvider);

        return factory.Object;
    }

    private static SiteSubscriptionAdminService CreateService(
        MockOdkContext context,
        IPaymentProviderFactory? paymentProviderFactory = null) => new(
        MockUnitOfWorkFactory.Create(context),
        CreateHtmlValidator(),
        paymentProviderFactory ?? Mock.Of<IPaymentProviderFactory>(),
        CreatePlatformProvider(),
        new SiteSubscriptionCooldown(months: 0));

    // Set up explicitly rather than left bare: a Mock.Of with no configured return hands back null from
    // Validate, which reads as a pass.
    private static IHtmlValidator CreateHtmlValidator() => Mock.Of<IHtmlValidator>(x =>
        x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()) == ServiceResult.Successful());

    private static IPlatformProvider CreatePlatformProvider() => Mock.Of<IPlatformProvider>(x =>
        x.GetName(It.IsAny<PlatformType>()) == PlatformName);

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}
