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

    [Test]
    public static void Build_EncodesQueryValues()
    {
        // Arrange
        var builder = UrlBuilder
            .Base(BaseUrl)
            .Query("key", "a b c");

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Be($"{BaseUrl}?key=a+b+c");
    }
}
