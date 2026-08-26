using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A member choosing their currency on the site-subscription page (<c>/account/subscription</c>, Default)
/// must persist - the chosen currency is a prerequisite for buying a site subscription. Regression cover
/// for a bug where the member-currency query required a member location + country, so a saved currency was
/// never reflected and the "choose currency" prompt kept reappearing after submit.
/// </summary>
[TestFixture]
public class SiteSubscriptionCurrencyTests : DefaultPageTest
{
    private static SitePaymentSettingsDataHelper PaymentSettings => new(E2ESettings.ConnectionString);

    [Test]
    public async Task ChooseCurrency_OnAccountSubscriptionPage_Persists()
    {
        // Arrange - a purchasable site subscription so the currency dropdown has an option, and a fresh
        // member who has no currency yet (a stubbed-geolocation signup has a location but no country).
        var paymentSettings = await PaymentSettings.GetStripeSettings(
            PlatformTypeId, E2ESettings.StripeAccountId(PlatformTypeId));
        await Provisioning.EnsurePurchasableSiteSubscription();
        var member = await Provisioning.NewAccount("currency-member");
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        var page = new SiteSubscriptionAccountPage(Page);
        await page.GoTo();
        (await page.IsCurrencyPromptShown())
            .Should().BeTrue("a member with no currency should be prompted to choose one");

        // Act - choose a currency and submit.
        await page.ChooseFirstCurrency();

        // Assert - reloading no longer prompts for a currency (the choice persisted).
        await page.GoTo();
        (await page.IsCurrencyPromptShown()).Should().BeFalse("the chosen currency should persist");
    }
}
