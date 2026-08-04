using FluentAssertions;
using Microsoft.Playwright;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// A member's stored locale (MemberPreferences.Locale - used to format request-independent output such as
/// emails) is refreshed from the request locale: every request runs RequestStore, which compares the stored
/// locale with the Accept-Language locale and enqueues a background job to update it when they differ. This
/// drives that end to end. Platform-agnostic, so it runs on Default only.
/// </summary>
[TestFixture]
public class MemberLocaleTests : DefaultPageTest
{
    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    [Test]
    public async Task MemberLocale_RequestInADifferentLocale_IsUpdatedInTheBackground()
    {
        // Arrange - a fresh member whose account captured a locale at sign-up.
        var member = await Provisioning.NewAccount(SharedAccounts.GroupMember);
        var memberId = await Members.GetMemberId(member.Email);
        var createdLocale = await Members.GetLocale(memberId);

        // A request locale guaranteed to differ from the one captured at creation.
        var requestLocale = createdLocale == "en-GB" ? "fr-FR" : "en-GB";

        // Act - the member signs in from a browser in the new locale. Each authenticated request runs
        // RequestStore, which enqueues the background locale update because the request locale now differs.
        await using var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = PlatformBaseUrl,
            Locale = requestLocale
        });
        var page = await context.NewPageAsync();
        await new LoginPage(page).LogIn(member.Email, member.Password);
        await page.Navigate("/");

        // Assert - the background job persists the new locale (polled, since the update is asynchronous).
        await WaitForStoredLocale(memberId, requestLocale);
    }

    private static async Task WaitForStoredLocale(Guid memberId, string expected)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            if (await Members.GetLocale(memberId) == expected)
            {
                return;
            }

            await Task.Delay(500);
        }

        // Final read for a clear failure message if the job never ran.
        (await Members.GetLocale(memberId)).Should().Be(expected);
    }
}
