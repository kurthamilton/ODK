using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Site-admin country management. The site-admin area is platform-agnostic, so this runs on Default only.
/// The test mutates shared country reference data, so it captures and restores the original locale.
/// </summary>
[TestFixture]
public class SiteAdminCountryTests : DefaultPageTest
{
    private static CountryDataHelper Countries => new(E2ESettings.ConnectionString);

    protected override string PlatformBaseUrl => E2ESettings.DefaultBaseUrl;

    [Test]
    public async Task UpdateCountry_AsSiteAdmin_PersistsSelectedLocale()
    {
        // Arrange - a seeded country and the site admin logged in. Capture the original locale to restore.
        var countryId = await Countries.GetFirstCountryId();
        var originalLocale = await Countries.GetDefaultLocale(countryId);

        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        try
        {
            // Act - set a locale from the country's table as the default.
            var selected = await new SiteAdminCountryPage(Page).SetFirstAvailableLocale(countryId);

            // Assert - the selected locale is persisted.
            (await Countries.GetDefaultLocale(countryId)).Should().Be(selected);
        }
        finally
        {
            // Restore the shared reference row.
            await Countries.SetDefaultLocale(countryId, originalLocale);
        }
    }
}
