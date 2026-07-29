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
        // Arrange - the site admin logged in. Find a seeded country that actually offers an alternate locale
        // to set: not every country maps to more than one .NET culture, so the alphabetically-first isn't
        // guaranteed to have a choice.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var countryPage = new SiteAdminCountryPage(Page);

        Guid? countryId = null;
        string? selected = null;
        foreach (var id in await Countries.GetCountryIdsByName(take: 30))
        {
            selected = await countryPage.GetFirstSettableLocale(id);
            if (selected != null)
            {
                countryId = id;
                break;
            }
        }

        countryId.Should().NotBeNull("at least one seeded country should offer an alternate locale to set");

        // Capture the original locale to restore the shared reference row afterwards.
        var originalLocale = await Countries.GetDefaultLocale(countryId.Value);

        try
        {
            // Act - set that locale as the country's default.
            await countryPage.SetLocaleAsDefault(countryId.Value, selected!);

            // Assert - the selected locale is persisted.
            (await Countries.GetDefaultLocale(countryId.Value)).Should().Be(selected);
        }
        finally
        {
            // Restore the shared reference row.
            await Countries.SetDefaultLocale(countryId.Value, originalLocale);
        }
    }
}
