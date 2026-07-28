using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Countries;
using ODK.Services.Countries.Models;
using ODK.Services.Exceptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Countries;

[Parallelizable]
public static class CountryAdminServiceTests
{
    [Test]
    public static async Task GetCountries_AsNonSiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.CreateCountry(isoCode2: "US");
        var member = context.CreateMember(siteAdmin: false);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act / Assert
        await FluentActions.Awaiting(() => service.GetCountries(CreateRequest(member)))
            .Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task GetCountries_AsSiteAdmin_ReturnsAllCountries()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.CreateCountry(isoCode2: "US");
        context.CreateCountry(isoCode2: "GB");
        var member = context.CreateMember(siteAdmin: true);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var countries = await service.GetCountries(CreateRequest(member));

        // Assert
        countries.Should().HaveCount(2);
    }

    [Test]
    public static void GetSupportedLocales_IncludesStoredLocaleOutsideRegion()
    {
        // Arrange - a US country whose stored locale isn't a US culture.
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        country.DefaultLocale = "fr-FR";
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var locales = service.GetSupportedLocales(country);

        // Assert - the region cultures plus the stored value, so it stays selectable.
        locales.Should().Contain("fr-FR");
        locales.Should().Contain("en-US");
    }

    [Test]
    public static void GetSupportedLocales_ReturnsRegionLocales()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var locales = service.GetSupportedLocales(country);

        // Assert
        locales.Should().Contain("en-US");
    }

    [Test]
    public static void ResolveDefaultLocale_WhenLocaleMissing_ReturnsDerivedLocale()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        country.DefaultLocale = null;
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var resolved = service.ResolveDefaultLocale(country);

        // Assert
        resolved.Should().NotBeNull();
        resolved.Should().Be(LocaleUtils.GetDefaultLocale("US"));
    }

    [Test]
    public static void ResolveDefaultLocale_WhenLocaleStored_ReturnsStoredLocale()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        country.DefaultLocale = "fr-FR";
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var resolved = service.ResolveDefaultLocale(country);

        // Assert
        resolved.Should().Be("fr-FR");
    }

    [Test]
    public static async Task UpdateCountry_AsNonSiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        var member = context.CreateMember(siteAdmin: false);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act / Assert
        await FluentActions.Awaiting(() => service.UpdateCountry(
                CreateRequest(member), country.Id, new CountryUpdateModel { DefaultLocale = "en-US" }))
            .Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task UpdateCountry_WithInvalidLocale_ReturnsFailureAndLeavesLocaleUnchanged()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        var member = context.CreateMember(siteAdmin: true);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var result = await service.UpdateCountry(
            CreateRequest(member), country.Id, new CountryUpdateModel { DefaultLocale = "not-a-locale" });

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Country>().Single(x => x.Id == country.Id).DefaultLocale.Should().BeNull();
    }

    [Test]
    public static async Task UpdateCountry_WithValidLocale_PersistsLocale()
    {
        // Arrange
        using var context = new MockOdkContext();
        var country = context.CreateCountry(isoCode2: "US");
        var member = context.CreateMember(siteAdmin: true);
        var service = new CountryAdminService(MockUnitOfWork.Create(context));

        // Act
        var result = await service.UpdateCountry(
            CreateRequest(member), country.Id, new CountryUpdateModel { DefaultLocale = "en-US" });

        // Assert
        result.Success.Should().BeTrue();
        context.Set<Country>().Single(x => x.Id == country.Id).DefaultLocale.Should().Be("en-US");
    }

    private static IMemberServiceRequest CreateRequest(Member member)
    {
        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(member);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(member);
        return mock.Object;
    }
}
