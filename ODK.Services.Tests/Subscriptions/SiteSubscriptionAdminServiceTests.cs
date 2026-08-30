using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Html;
using ODK.Services.Payments;
using ODK.Services.Subscriptions;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionAdminServiceTests
{
    [Test]
    public static async Task AddSiteSubscription_ExistingProduct_ReusesIt()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var product = context.CreateSitePaymentProduct(externalId: "existing-product");
        var paymentProvider = CreatePaymentProvider(createdProductId: "new-product");
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.AddSiteSubscription(
            SiteAdminRequest(context), CreateModel(free: false));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SitePaymentProduct>().Should().HaveCount(1);
        paymentProvider.Verify(x => x.GetOrCreatePlatformProduct(It.IsAny<PlatformType>()), Times.Never);

        context.Set<SiteSubscription>()
            .Single(x => x.Id == result.Value).SitePaymentProductId.Should().Be(product.Id);
    }

    [Test]
    public static async Task AddSiteSubscription_NoExistingProduct_CreatesOneNamedForThePlatform()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var paymentProvider = CreatePaymentProvider(createdProductId: "new-product");
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.AddSiteSubscription(
            SiteAdminRequest(context), CreateModel(free: false));

        // Assert
        result.Success.Should().BeTrue();
        paymentProvider.Verify(x => x.GetOrCreatePlatformProduct(PlatformType.Default), Times.Once);

        var product = context.Set<SitePaymentProduct>().Single();
        product.ExternalId.Should().Be("new-product");
        product.Platform.Should().Be(PlatformType.Default);
        product.PaymentProvider.Should().Be(PaymentProviderType.Stripe);

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
        var product = context.CreateSitePaymentProduct(externalId: "platform-product");
        var subscription = context.CreateSiteSubscription(
            sitePaymentProduct: product);
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
    public static async Task DeleteSiteSubscriptionPrice_MemberSubscribedOnIt_Fails()
    {
        /* Arrange - the record names the price the member paid, so the price outlives it. Asserted on the
           provider too: a plan deactivated for a price still in use would stop the renewal it backs. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var price = context.CreateSiteSubscriptionPrice(subscription, amount: 5);
        context.CreateMemberSiteSubscription(
            context.CreateMember(), subscription, siteSubscriptionPrice: price);

        var paymentProvider = CreatePaymentProvider(createdProductId: null);
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.DeleteSiteSubscriptionPrice(
            SiteAdminRequest(context), subscription.Id, price.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscriptionPrice>().Any(x => x.Id == price.Id).Should().BeTrue();
        paymentProvider.Verify(
            x => x.DeactivateSubscriptionPlan(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task DeleteSiteSubscriptionPrice_NoMemberRecords_DeletesItAndDeactivatesThePlan()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var price = context.CreateSiteSubscriptionPrice(subscription, amount: 5);

        var paymentProvider = CreatePaymentProvider(createdProductId: null);
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.DeleteSiteSubscriptionPrice(
            SiteAdminRequest(context), subscription.Id, price.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SiteSubscriptionPrice>().Any(x => x.Id == price.Id).Should().BeFalse();
        paymentProvider.Verify(
            x => x.DeactivateSubscriptionPlan(price.ExternalId!), Times.Once);
    }

    [Test]
    public static async Task DeleteSiteSubscription_DefaultSubscription_Fails()
    {
        // Arrange - every new account is put on the default, so the platform cannot be left without one.
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription(free: true);
        subscription.Default = true;
        var service = CreateService(context);

        // Act
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Any(x => x.Id == subscription.Id).Should().BeTrue();
    }

    [Test]
    public static async Task DeleteSiteSubscription_FallbackForAnotherSubscription_Fails()
    {
        /* Arrange - a fallback is a column on the subscription that names it, so deleting the named row would
           fail on the foreign key rather than clear it. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var dependent = context.CreateSiteSubscription();
        dependent.FallbackSiteSubscriptionId = subscription.Id;
        var service = CreateService(context);

        // Act
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Any(x => x.Id == subscription.Id).Should().BeTrue();
    }

    [Test]
    public static async Task DeleteSiteSubscription_MemberHasBeenOnIt_Fails()
    {
        // Arrange - the record is part of that member's payment history, so it is not the platform's to erase.
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateMemberSiteSubscription(context.CreateMember(), subscription);
        var service = CreateService(context);

        // Act
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Any(x => x.Id == subscription.Id).Should().BeTrue();
    }

    [Test]
    public static async Task DeleteSiteSubscription_NothingDependsOnIt_Deletes()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var service = CreateService(context);

        // Act
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<SiteSubscription>().Any(x => x.Id == subscription.Id).Should().BeFalse();
    }

    [Test]
    public static async Task DeleteSiteSubscription_WithPrices_Fails()
    {
        /* Arrange - a price has a plan at the payment provider that has to be deactivated with it, which is
           what deleting the price does, so the prices go first and by hand. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateSiteSubscriptionPrice(subscription, amount: 5);
        var service = CreateService(context);

        // Act
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<SiteSubscription>().Any(x => x.Id == subscription.Id).Should().BeTrue();
    }

    [Test]
    public static async Task GetSubscriptionEditViewModel_PriceMemberSubscribedOn_CannotBeDeleted()
    {
        /* Arrange - two prices, one of them subscribed on. The page only offers a delete the service will
           accept, so this asserts the flags the rows are rendered from and that acting on each agrees. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var used = context.CreateSiteSubscriptionPrice(subscription, amount: 5);
        var unused = context.CreateSiteSubscriptionPrice(subscription, amount: 10);
        context.CreateMemberSiteSubscription(
            context.CreateMember(), subscription, siteSubscriptionPrice: used);

        var paymentProvider = CreatePaymentProvider(createdProductId: null);
        var service = CreateService(context, CreatePaymentProviderFactory(paymentProvider.Object));

        // Act
        var viewModel = await service.GetSubscriptionEditViewModel(
            SiteAdminRequest(context), subscription.Id);

        var usedResult = await service.DeleteSiteSubscriptionPrice(
            SiteAdminRequest(context), subscription.Id, used.Id);
        var unusedResult = await service.DeleteSiteSubscriptionPrice(
            SiteAdminRequest(context), subscription.Id, unused.Id);

        // Assert
        viewModel.Prices.Single(x => x.Id == used.Id).CanDelete.Should().BeFalse();
        viewModel.Prices.Single(x => x.Id == unused.Id).CanDelete.Should().BeTrue();

        usedResult.Success.Should().BeFalse();
        unusedResult.Success.Should().BeTrue();
    }

    [Test]
    public static async Task GetSiteSubscriptionSiteAdminListItems_DefaultSubscription_CannotBeDeleted()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription(free: true);
        subscription.Default = true;
        var service = CreateService(context);

        // Act
        var items = await service.GetSiteSubscriptionSiteAdminListItems(SiteAdminRequest(context));

        // Assert
        items.Single(x => x.Id == subscription.Id).CanDelete.Should().BeFalse();
    }

    [Test]
    public static async Task GetSiteSubscriptionSiteAdminListItems_FallbackForAnotherSubscription_CannotBeDeleted()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var dependent = context.CreateSiteSubscription();
        dependent.FallbackSiteSubscriptionId = subscription.Id;
        var service = CreateService(context);

        // Act
        var items = await service.GetSiteSubscriptionSiteAdminListItems(SiteAdminRequest(context));

        // Assert
        items.Single(x => x.Id == subscription.Id).CanDelete.Should().BeFalse();
        items.Single(x => x.Id == dependent.Id).CanDelete.Should().BeTrue();
    }

    [Test]
    public static async Task GetSiteSubscriptionSiteAdminListItems_MemberHasBeenOnIt_CannotBeDeleted()
    {
        /* Arrange - the record has no expiry set, so it does not count towards the active total either. The
           block is on any record ever written, not on the ones still in force. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateMemberSiteSubscription(context.CreateMember(), subscription);
        var service = CreateService(context);

        // Act
        var items = await service.GetSiteSubscriptionSiteAdminListItems(SiteAdminRequest(context));

        // Assert
        var item = items.Single(x => x.Id == subscription.Id);
        item.CanDelete.Should().BeFalse();
        item.ActiveCount.Should().Be(0);
    }

    [Test]
    public static async Task GetSiteSubscriptionSiteAdminListItems_NothingDependsOnIt_CanBeDeleted()
    {
        /* Arrange - the list only offers a delete the service will accept, so this asserts both: the flag the
           row is rendered from, and that acting on it succeeds. */
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        var service = CreateService(context);

        // Act
        var items = await service.GetSiteSubscriptionSiteAdminListItems(SiteAdminRequest(context));
        var result = await service.DeleteSiteSubscription(SiteAdminRequest(context), subscription.Id);

        // Assert
        items.Single(x => x.Id == subscription.Id).CanDelete.Should().BeTrue();
        result.Success.Should().BeTrue();
    }

    [Test]
    public static async Task GetSiteSubscriptionSiteAdminListItems_WithPrices_CannotBeDeleted()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var subscription = context.CreateSiteSubscription();
        context.CreateSiteSubscriptionPrice(subscription, amount: 5);
        var service = CreateService(context);

        // Act
        var items = await service.GetSiteSubscriptionSiteAdminListItems(SiteAdminRequest(context));

        // Assert
        items.Single(x => x.Id == subscription.Id).CanDelete.Should().BeFalse();
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
        Name = "Test Subscription"
    };

    private static MockOdkContext CreateMockOdkContext() => new();

    private static Mock<IPaymentProvider> CreatePaymentProvider(string? createdProductId)
    {
        var paymentProvider = new Mock<IPaymentProvider>();

        paymentProvider
            .Setup(x => x.ActivateSubscriptionPlan(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult.Successful());

        paymentProvider
            .Setup(x => x.GetOrCreatePlatformProduct(It.IsAny<PlatformType>()))
            .ReturnsAsync(createdProductId ?? "external-product-id");

        paymentProvider
            .Setup(x => x.CreateSubscriptionPlan(It.IsAny<ExternalSubscriptionPlan>()))
            .ReturnsAsync("plan-external-id");

        return paymentProvider;
    }

    private static IPaymentProviderFactory CreatePaymentProviderFactory(IPaymentProvider paymentProvider)
    {
        var factory = new Mock<IPaymentProviderFactory>();

        factory
            .Setup(x => x.GetPaymentProvider(
                It.IsAny<PaymentProviderType>(), It.IsAny<PlatformType>()))
            .Returns(paymentProvider);

        return factory.Object;
    }

    private static SiteSubscriptionAdminService CreateService(
        MockOdkContext context,
        IPaymentProviderFactory? paymentProviderFactory = null) => new(
        MockUnitOfWorkFactory.Create(context),
        CreateHtmlValidator(),
        paymentProviderFactory ?? Mock.Of<IPaymentProviderFactory>(),
        new SiteSubscriptionCooldown(months: 0),
        new SiteSubscriptionAdminServiceSettings { PaymentProvider = PaymentProviderType.Stripe });

    // Set up explicitly rather than left bare: a Mock.Of with no configured return hands back null from
    // Validate, which reads as a pass.
    private static IHtmlValidator CreateHtmlValidator() => Mock.Of<IHtmlValidator>(x =>
        x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()) == ServiceResult.Successful());

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Environment == EnvironmentType.Dev &&
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}
