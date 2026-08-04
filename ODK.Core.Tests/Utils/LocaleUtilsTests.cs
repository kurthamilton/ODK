using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class LocaleUtilsTests
{
    [Test]
    public static void GetPreferredLocale_FirstValidSpecificCulture_IsReturned()
    {
        // Act / Assert - highest-priority candidate that is a specific culture wins.
        LocaleUtils.GetPreferredLocale(["fr-FR", "en-GB"]).Should().Be("fr-FR");
    }

    [Test]
    public static void GetPreferredLocale_SkipsNeutralCulture_PicksNextSpecific()
    {
        // Act / Assert - a region-less "en" is skipped so the default locale can supply the region.
        LocaleUtils.GetPreferredLocale(["en", "en-GB"]).Should().Be("en-GB");
    }

    [Test]
    public static void GetPreferredLocale_SkipsWildcardAndUnknown()
    {
        // Act / Assert
        LocaleUtils.GetPreferredLocale(["*", "not-a-locale", "en-US"]).Should().Be("en-US");
    }

    [Test]
    public static void GetPreferredLocale_CanonicalisesName()
    {
        // Act / Assert - the returned name is the runtime's canonical form.
        LocaleUtils.GetPreferredLocale(["EN-gb"]).Should().Be("en-GB");
    }

    [Test]
    public static void GetPreferredLocale_NoValidSpecificCulture_ReturnsNull()
    {
        // Act / Assert - neutral, unknown and wildcard candidates all fall through.
        LocaleUtils.GetPreferredLocale(["en", "fr", "not-a-locale", "*"]).Should().BeNull();
    }

    [Test]
    public static void GetPreferredLocale_Empty_ReturnsNull()
    {
        // Act / Assert
        LocaleUtils.GetPreferredLocale([]).Should().BeNull();
    }

    [Test]
    public static void GetPreferredLocale_Result_IsAcceptedByShortDatePattern()
    {
        // Consistency guard: anything returned is a culture LocaleService (via GetShortDatePattern) accepts.
        var locale = LocaleUtils.GetPreferredLocale(["fr-FR", "en-GB"]);
        LocaleUtils.GetShortDatePattern(locale).Should().NotBeNull();
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
}
