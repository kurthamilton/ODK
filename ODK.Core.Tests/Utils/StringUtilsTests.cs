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

    [TestCase("abc", "/", ExpectedResult = "abc/")]
    [TestCase("abc/", "/", ExpectedResult = "abc/")]
    [TestCase(null, "/", ExpectedResult = "/")]
    [TestCase("", "/", ExpectedResult = "/")]
    public static string EnsureTrailing(string? text, string trailingText) => text.EnsureTrailing(trailingText);

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

    [TestCase("#a", ExpectedResult = true)]
    [TestCase("#a1", ExpectedResult = true)]
    [TestCase("#a1-no-special-chars", ExpectedResult = false)]
    [TestCase("#a1 no spaces", ExpectedResult = false)]
    [TestCase("#a1<script>alert('HACKED!');</script>", ExpectedResult = false)]
    public static bool IsHashtag(string text) => StringUtils.IsHashtag(text);

    [Test]
    public static void IsolateHashtags()
    {
        // Arrange
        const string text = "This is paragraph 1. Hello, World! #abc #xyz This is paragraph 2.";

        // Act
        var result = StringUtils.IsolateHashtags(text);

        // Assert
        result.Should().Equal([
            "This is paragraph 1. Hello, World! ",
            "#abc",
            " ",
            "#xyz",
            " This is paragraph 2."]);
    }
}