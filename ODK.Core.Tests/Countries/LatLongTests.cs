using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Countries;

namespace ODK.Core.Tests.Countries;

[Parallelizable]
public static class LatLongTests
{
    [Test]
    public static void Constructor_ReadsLatitudeThenLongitude()
    {
        // Arrange - the two differ, and differ in sign, so an argument swap cannot pass.
        // Act
        var result = new LatLong(53.3811, -1.4701);

        // Assert
        result.Lat.Should().Be(53.3811);
        result.Long.Should().Be(-1.4701);
    }

    [Test]
    public static void FromCoords_BothGiven_ReadsLatitudeThenLongitude()
    {
        // Act
        var result = LatLong.FromCoords(53.3811, -1.4701);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Lat.Should().Be(53.3811);
        result.Value.Long.Should().Be(-1.4701);
    }

    [TestCase(null, -1.4701)]
    [TestCase(53.3811, null)]
    [TestCase(null, null)]
    public static void FromCoords_EitherMissing_ReturnsNull(double? lat, double? @long)
    {
        // Act - half a position is not a position.
        var result = LatLong.FromCoords(lat, @long);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void IsDefault_BothZero_IsTrue()
    {
        // Arrange - the unset pair, which is also a real place in the Atlantic. Nothing here can tell the
        // two apart, which is why an unset location is not stored rather than stored as 0,0.
        // Act
        var result = new LatLong(0, 0).IsDefault;

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(53.3811, 0)]
    [TestCase(0, -1.4701)]
    [TestCase(53.3811, -1.4701)]
    public static void IsDefault_EitherSet_IsFalse(double lat, double @long)
    {
        // Act
        var result = new LatLong(lat, @long).IsDefault;

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// The pair is split on commas at the other end, so a decimal comma would arrive as four values
    /// rather than two. Formatting follows the request's locale everywhere else, which is what makes
    /// this worth pinning.
    /// </summary>
    [SetCulture("de-DE")]
    [Test]
    public static void ToString_CultureWithDecimalComma_UsesDecimalPoints()
    {
        // Act
        var result = new LatLong(53.3811, -1.4701).ToString();

        // Assert
        result.Should().Be("53.3811,-1.4701");
    }

    [Test]
    public static void ToString_IsLatitudeThenLongitude()
    {
        // Arrange - the order the Maps embed and the map-query hidden field both expect.
        // Act
        var result = new LatLong(53.3811, -1.4701).ToString();

        // Assert
        result.Should().Be("53.3811,-1.4701");
    }
}
