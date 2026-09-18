using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Web;

namespace ODK.Core.Tests.Web;

[Parallelizable]
public static class UrlBuilderTests
{
    private const string BaseUrl = "https://example.com";

    [Test]
    public static void Build_BaseUrlOnly()
    {
        // Arrange
        var builder = UrlBuilder.Base(BaseUrl);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be(BaseUrl);
    }

    [Test]
    public static void Build_BaseUrlWithPath()
    {
        // Arrange
        var builder = UrlBuilder
            .Base(BaseUrl)
            .Path("/path");

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be($"{BaseUrl}/path");
    }

    [Test]
    public static void Build_BaseUrlWithPathAndQuery()
    {
        // Arrange
        var builder = UrlBuilder
            .Base(BaseUrl)
            .Path("/path")
            .Query("a", "1")
            .Query("b", "2");

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be($"{BaseUrl}/path?a=1&b=2");
    }

    /* Percent-encoding, not form encoding: a space is %20 and never +, which means only one thing in a
       URL and it is not a space. It also matches what encodeURIComponent produces, so a value a script
       appends and one the server appends come out the same. */
    [TestCase("a b c", "a%20b%20c")]
    [TestCase("a+b", "a%2Bb")]
    [TestCase("a&b=c", "a%26b%3Dc")]
    [TestCase("Aa0-._~", "Aa0-._~")]
    public static void Build_EncodesQueryValues(string value, string expected)
    {
        // Arrange
        var builder = UrlBuilder
            .Base(BaseUrl)
            .Query("key", value);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be($"{BaseUrl}?key={expected}");
    }

    /// <summary>
    /// A value is encoded, so a placeholder put through here comes back encoded too and no later
    /// substitution can find it. Anything meant to be filled in afterwards is appended to a built url
    /// rather than passed through as a value - see the Google map component.
    /// </summary>
    [Test]
    public static void Build_EncodesAValueThatLooksLikeAPlaceholder()
    {
        // Arrange
        var builder = UrlBuilder
            .Base(BaseUrl)
            .Query("q", "{query}");

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be($"{BaseUrl}?q=%7Bquery%7D");
    }
}
