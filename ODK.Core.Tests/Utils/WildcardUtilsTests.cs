using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class WildcardUtilsTests
{
    [TestCase("/abc", "/abc", ExpectedResult = true)]
    [TestCase("/abc", "/abcd", ExpectedResult = false)]
    [TestCase("/ABC", "/abc", ExpectedResult = true)]
    [TestCase("/abc*", "/abcd", ExpectedResult = true)]
    [TestCase("/abc*", "/x/abc", ExpectedResult = false)]
    [TestCase("*/abc", "/x/abc", ExpectedResult = true)]
    [TestCase("*/abc", "/abc/x", ExpectedResult = false)]
    [TestCase("*abc*", "/x/abcd", ExpectedResult = true)]
    [TestCase("*abc*", "/x/def", ExpectedResult = false)]
    [TestCase("*SPIDER*", "this is a spider user agent", ExpectedResult = true)]
    public static bool Matches_Wildcards(string rule, string value)
        => WildcardUtils.Matches(rule, value);

    [Test]
    public static void Matches_WildcardAlone_MatchesAnything()
    {
        /* Arrange - "*" is both wildcards at once with nothing between them. Taking rule[1..^1] of it asks for a
           range ending before it starts, so this threw rather than matching everything. */

        // Act
        var result = WildcardUtils.Matches("*", "/anything");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public static void Matches_EmptyValue_MatchesOnlyAWildcard()
    {
        // Act / Assert - nothing to compare against, so only a rule that looks for nothing can match.
        Assert.That(WildcardUtils.Matches("*", string.Empty), Is.True);
        Assert.That(WildcardUtils.Matches("/abc", string.Empty), Is.False);
        Assert.That(WildcardUtils.Matches("*abc", string.Empty), Is.False);
    }
}
