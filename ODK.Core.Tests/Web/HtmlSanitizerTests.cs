using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Web;

namespace ODK.Core.Tests.Web;

[Parallelizable]
public static class HtmlSanitizerTests
{
    [TestCase("embed")]
    [TestCase("form")]
    [TestCase("iframe")]
    [TestCase("object")]
    [TestCase("script")]
    public static void Sanitize_EncodesSelfClosingBlacklistedTags(string tag)
    {
        // Arrange
        var html = $"<{tag}/>";
        var sanitizer = new HtmlSanitizer();

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        var expected = $"&lt;{tag}/&gt;";

        result.Should().Be(expected);
    }

    [TestCase("embed")]
    [TestCase("form")]
    [TestCase("iframe")]
    [TestCase("object")]
    [TestCase("script")]
    public static void Sanitize_EncodesBlacklistedTags(string tag)
    {
        // Arrange
        var html = $"<{tag}>Content</{tag}>";
        var sanitizer = new HtmlSanitizer();

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        var expected = $"&lt;{tag}&gt;Content&lt;/{tag}&gt;";

        result.Should().Be(expected);
    }
}
