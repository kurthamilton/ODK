using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

public static class StringUtilsTests
{
    [TestCase("a2c")]
    [TestCase(".a.2.c.")]
    [TestCase("\\a.2.c/")]
    [TestCase("\\a.2.c/")]
    [TestCase("#a.2.c###################")]
    public static void AlphaNumericReturnsAlphaNumericString(string text)
    {
        text = StringUtils.AlphaNumeric(text);

        text.Should().Be("a2c");
    }

    [Test]
    public static void Interpolate_ReplacesTokens()
    {
        string text = "The {a} brown {b}";

        string result = StringUtils.Interpolate(text, new Dictionary<string, string>
        {
            {"a", "quick"},
            {"b", "fox"}
        });

        result.Should().Be("The quick brown fox");
    }

    [Test]
    public static void Interpolate_ReplacesAllInstances()
    {
        string text = "The {a} brown {a} {b}";

        string result = StringUtils.Interpolate(text, new Dictionary<string, string>
        {
            {"a", "quick"},
            {"b", "fox"}
        });

        result.Should().Be("The quick brown quick fox");
    }

    [Test]
    public static void Interpolate_IgnoresMissingTokenValues()
    {
        string text = "The {a} brown {b}";

        string result = StringUtils.Interpolate(text, new Dictionary<string, string>
        {
            {"a", "quick"}
        });

        result.Should().Be("The quick brown {b}");
    }

    [Test]
    public static void Tokens_ReturnsMultipleTokens()
    {
        IEnumerable<string> tokens = StringUtils.Tokens("This {a} a {b} {c}");

        tokens.Should().BeEquivalentTo("a", "b", "c");
    }
}
