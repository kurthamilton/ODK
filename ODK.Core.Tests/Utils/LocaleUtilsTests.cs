using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class LocaleUtilsTests
{
    [Test]
    public static void GetDefaultLocale_GbCode_ResolvesToADayFirstCulture()
    {
        // Act - a region can map to several cultures, so assert the resulting date order, not the name.
        var locale = LocaleUtils.GetDefaultLocale("GB");
        var pattern = LocaleUtils.GetShortDatePattern(locale);

        // Assert - UK date order is day before month.
        pattern.Should().NotBeNull();
        pattern!.IndexOf('d').Should().BeLessThan(pattern.IndexOf('M'));
    }

    [Test]
    public static void GetDefaultLocale_UnknownIsoCode_ReturnsNull()
    {
        // Act
        var result = LocaleUtils.GetDefaultLocale("ZZ");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void GetLocalesForCountry_UnknownIsoCode_ReturnsEmpty()
    {
        // Act / Assert
        LocaleUtils.GetLocalesForCountry("ZZ").Should().BeEmpty();
    }

    [Test]
    public static void GetLocalesForCountry_UsCode_IncludesEnUsAndLeadsWithTheDefault()
    {
        // Act
        var locales = LocaleUtils.GetLocalesForCountry("US");

        // Assert - the full set for the region, led by the derived default.
        locales.Should().Contain("en-US");
        locales.First().Should().Be(LocaleUtils.GetDefaultLocale("US"));
    }

    [Test]
    public static void GetDefaultLocale_UsCode_ResolvesToAMonthFirstCulture()
    {
        // Act
        var locale = LocaleUtils.GetDefaultLocale("US");
        var pattern = LocaleUtils.GetShortDatePattern(locale);

        // Assert - US date order is month before day.
        pattern.Should().NotBeNull();
        pattern!.IndexOf('M').Should().BeLessThan(pattern.IndexOf('d'));
    }

    [Test]
    public static void GetShortDatePattern_EnGb_ReturnsDayMonthYear()
    {
        // Act
        var pattern = LocaleUtils.GetShortDatePattern("en-GB");

        // Assert
        pattern.Should().Be("dd/MM/yyyy");
    }

    [Test]
    public static void GetShortDatePattern_EnUs_ReturnsMonthDayYear()
    {
        // Act
        var pattern = LocaleUtils.GetShortDatePattern("en-US");

        // Assert
        pattern.Should().Be("M/d/yyyy");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void GetShortDatePattern_MissingOrBlank_ReturnsNull(string? localeName)
    {
        // Act
        var result = LocaleUtils.GetShortDatePattern(localeName);

        // Assert
        result.Should().BeNull();
    }

    [TestCase("en-GB")]
    [TestCase("en-US")]
    public static void IsValidLocale_KnownCulture_ReturnsTrue(string localeName)
    {
        // Act / Assert
        LocaleUtils.IsValidLocale(localeName).Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not-a-locale")]
    public static void IsValidLocale_UnknownOrBlank_ReturnsFalse(string? localeName)
    {
        // Act / Assert
        LocaleUtils.IsValidLocale(localeName).Should().BeFalse();
    }
}
