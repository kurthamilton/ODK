using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Site-admin Stripe scenario: create a site subscription (with the MemberSubscriptions "Paid
/// subscriptions" feature) and add a recurring price. Runs on the Default platform (:8125), so the new
/// subscription's platform is Default. Creating the subscription and adding a paid price call the real
/// Stripe API, so the app under test needs live Stripe keys configured for the Default platform. Not
/// webhook-dependent - no payment is taken.
/// </summary>
[TestFixture]
public class SiteSubscriptionTests : DefaultPageTest
{
    // PlatformTypeId for the Default platform (the port this fixture runs against).
    private const int PlatformTypeDefault = 1;

    // Numeric SiteFeatureType value for MemberSubscriptions ("Paid subscriptions").
    private const int SiteFeatureMemberSubscriptions = 5;

    private static SiteSubscriptionDataHelper Subscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task CreateSubscription_WithMemberSubscriptionsFeatureAndPrice_Persists()
    {
        // Arrange - the site admin. Creating a priced subscription calls the real Stripe API, on whichever
        // account the app's own configuration names.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        var name = $"{SiteSubscriptionDataHelper.TestNamePrefix}{Guid.NewGuid():N}";
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        // Act - create the subscription with the "Paid subscriptions" feature, then add a monthly price.
        var page = new SiteAdminSubscriptionsPage(Page);
        await page.CreateSubscription(
            name, "E2E paid subscription", groupLimit: 1, memberLimit: 10,
            featureIds: new[] { SiteFeatureMemberSubscriptions });
        await page.AddPrice("GBP", "Monthly", 5m);

        // Assert - the subscription persisted for the Default platform, with the feature and a price.
        var id = await Subscriptions.GetId(name, PlatformTypeDefault);
        id.Should().NotBeNull();
        (await Subscriptions.HasFeature(id!.Value, SiteFeatureMemberSubscriptions)).Should().BeTrue();
        (await Subscriptions.PriceCount(id.Value)).Should().BeGreaterThan(0);
    }
}
