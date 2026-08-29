using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter-admin Stripe scenario: an owner creates a non-recurring chapter subscription on the Default
/// platform. Prerequisites are seeded directly - the owner's MemberSubscriptions-featured site subscription
/// and a set-up Stripe Connect payment account (with a fake acct_) - so no hosted onboarding is driven.
/// Creating the subscription calls the real Stripe API on the platform account (product + one-off price),
/// so this needs live Stripe keys. It's neither webhook-dependent nor a purchase, so a fake connected
/// account is sufficient here (a real onboarded acct_ is only needed for the member-purchase test).
/// </summary>
[TestFixture]
public class ChapterSubscriptionTests : DefaultPageTest
{
    private static ChapterPaymentAccountDataHelper ChapterPaymentAccounts => new(E2ESettings.ConnectionString);

    private static ChapterSubscriptionDataHelper ChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    [Test]
    public async Task CreateNonRecurringChapterSubscription_Persists()
    {
        // Arrange - a chapter whose owner has the MemberSubscriptions site feature and a set-up payment
        // account (both seeded). The chapter is local: the test mutates it by adding a subscription.
        var siteSubscription = await Provisioning.EnsurePurchasableSiteSubscription();

        var owner = await Provisioning.NewAccount("chapter-subscription-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2echapsub{Guid.NewGuid():N}");
        var ownerId = await Members.GetMemberId(owner.Email);

        await MemberSubscriptions.EnsureActive(ownerId, siteSubscription.Id, siteSubscription.PriceId);
        await ChapterPaymentAccounts.EnsureSetupComplete(
            group.ChapterId, ownerId, "acct_e2e_fake", E2ESettings.EnvironmentTypeId);

        await new LoginPage(Page).LogIn(owner.Email, owner.Password);

        // Act - create a non-recurring chapter subscription.
        var name = $"e2e-chaptersub-{Guid.NewGuid():N}";
        var routes = PlatformRoutes.Default(group);
        await new ChapterSubscriptionAdminPage(Page).CreateSubscription(
            routes.SubscriptionCreate, name, title: "E2E Membership", description: "E2E chapter subscription",
            amount: 5m, durationMonths: 1, recurring: false);

        // Assert - it persisted as non-recurring, with a Stripe price created on the platform account.
        (await ChapterSubscriptions.IsRecurring(group.ChapterId, name)).Should().BeFalse();
        (await ChapterSubscriptions.GetExternalId(group.ChapterId, name)).Should().NotBeNullOrEmpty();
    }
}
