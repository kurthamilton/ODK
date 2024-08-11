using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Countries;

namespace ODK.Core.Tests.Countries;

[Parallelizable]
public static class LatLongTests
{
    [Test]
    public static void DistanceFrom_SameDistance_ReturnsZero()
    {
        // Arrange
        var location1 = new LatLong(50, 5);
        var location2 = new LatLong(50, 5);

        // Act
        var result = location1.DistanceFrom(location2);

        // Assert
        result.Should().Be(0);
    }

    [TestCase(51.7520, 1.2577, 51.5072, 0.1276, 82_615)] // Oxford - London
    public static void DistanceFrom(double lat1, double long1, double lat2, double long2, 
        double expectedApproxDistance)
    {
        // Arrange
        var location1 = new LatLong(lat1, long1);
        var location2 = new LatLong(lat2, long2);

        // Act
        var result = location1.DistanceFrom(location2);

        // Assert
        result.Should().BeApproximately(expectedApproxDistance, 1000);
    }
}
