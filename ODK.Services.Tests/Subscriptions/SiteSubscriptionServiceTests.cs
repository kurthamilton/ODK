using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Payments;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Subscriptions;

[Parallelizable]
public static class SiteSubscriptionServiceTests
{
    [Test]
    public static async Task StartSiteSubscriptionCheckout_PriceOnAnotherPlatform_Throws()
    {
        /* Arrange - the price id is posted by the browser, so a Drunken Knitwits plan can be submitted to a
           Group Squirrel checkout. The list it should have been chosen from is this platform's, and nothing
           downstream confines it: the payment would be booked against the plan's own platform. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var subscription = context.CreateSiteSubscription(platform: PlatformType.DrunkenKnitwits);
        var price = context.CreateSiteSubscriptionPrice(subscription);

        var service = CreateService(context);
        var request = Mock.Of<IMemberServiceRequest>(x =>
            x.Platform == PlatformType.Default &&
            x.CurrentMember == member);

        // Act
        var act = () => service.StartSiteSubscriptionCheckout(request, price.Id, returnPath: "/");

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    private static MockOdkContext CreateMockOdkContext() => new();

    private static SiteSubscriptionService CreateService(MockOdkContext context) => new(
        MockUnitOfWorkFactory.Create(context),
        Mock.Of<IPaymentProviderFactory>(),
        Mock.Of<IPaymentService>());
}
