using FluentAssertions;
using ODK.Services.Integrations.Authentication;

namespace ODK.Services.Integrations.Tests.Authentication;

[Parallelizable]
public static class HibpBreachedPasswordCheckerTests
{
    // Suffix of SHA-1("password") after the 5-char prefix; count is non-zero in a real response.
    private const string Body =
        "1E4C9B93F3F0682250B6CF8331B7EE68FD8:100\r\n" +
        "0018A45C4D1DEF81644B54AB7F969B88D65:1\r\n" +
        "00D4F6E8FA6EECAD2A3AA415EEC418D38EC:0\r\n";

    [Test]
    public static void ContainsSuffix_PresentWithNonZeroCount_ReturnsTrue()
    {
        HibpBreachedPasswordChecker.ContainsSuffix(Body, "1E4C9B93F3F0682250B6CF8331B7EE68FD8")
            .Should().BeTrue();
    }

    [Test]
    public static void ContainsSuffix_CaseInsensitive_ReturnsTrue()
    {
        HibpBreachedPasswordChecker.ContainsSuffix(Body, "1e4c9b93f3f0682250b6cf8331b7ee68fd8")
            .Should().BeTrue();
    }

    [Test]
    public static void ContainsSuffix_NotPresent_ReturnsFalse()
    {
        HibpBreachedPasswordChecker.ContainsSuffix(Body, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
            .Should().BeFalse();
    }

    [Test]
    public static void ContainsSuffix_PresentButZeroCount_ReturnsFalse()
    {
        // Padding rows have a count of 0 and must not count as a match.
        HibpBreachedPasswordChecker.ContainsSuffix(Body, "00D4F6E8FA6EECAD2A3AA415EEC418D38EC")
            .Should().BeFalse();
    }
}
