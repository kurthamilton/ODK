using FluentAssertions;
using NUnit.Framework;
using ODK.Web.Razor.Helpers;

namespace ODK.Web.Razor.Tests.Helpers;

[Parallelizable]
public static class FlatpickrDateFormatTests
{
    [TestCase("dd/MM/yyyy", "d/m/Y")]
    [TestCase("M/d/yyyy", "n/j/Y")]
    [TestCase("yyyy-MM-dd", "Y-m-d")]
    [TestCase("d.M.yyyy", "j.n.Y")]
    [TestCase("dd/MM/yy", "d/m/y")]
    public static void FromShortDatePattern_TranslatesTokensAndKeepsSeparators(string pattern, string expected)
    {
        // Act
        var result = FlatpickrDateFormat.FromShortDatePattern(pattern);

        // Assert
        result.Should().Be(expected);
    }
}
