using FluentAssertions;
using ODK.Core.Countries;
using ODK.Services.Integrations.Geolocation;

namespace ODK.Services.Integrations.Tests.Geolocation;

[Parallelizable]
public static class LatLongCalculatorTests
{
    private static readonly DistanceUnit MetresUnit = CreateDistanceUnit(metres: 1);
    private static readonly DistanceUnit KilometresUnit = CreateDistanceUnit(metres: 1000);

    [TestCase(51.7520, 1.2577, 51.5072, 0.1276, 82_615)] // Oxford - London
    public static void CalculateDistanceBetween(double lat1, double long1, double lat2, double long2,
        double expectedApproxDistance)
    {
        // Arrange
        var location1 = new LatLong(lat1, long1);
        var location2 = new LatLong(lat2, long2);

        // Act
        var result = new LatLongCalculator().CalculateDistanceBetween(location1, location2, MetresUnit);

        // Assert
        result.Should().BeApproximately(expectedApproxDistance, 1000);
    }

    [TestCase(51.7520, 1.2577, 51.5072, 0.1276, 82)] // Oxford - London
    public static void CalculateDistanceBetween_Kilometres(double lat1, double long1, double lat2, double long2,
        double expectedApproxDistance)
    {
        // Arrange
        var location1 = new LatLong(lat1, long1);
        var location2 = new LatLong(lat2, long2);

        // Act
        var result = new LatLongCalculator().CalculateDistanceBetween(location1, location2, KilometresUnit);

        // Assert
        result.Should().BeApproximately(expectedApproxDistance, 1);
    }

    [Test]
    public static void CalculateDistanceBetween_SameLocation_ReturnsZero()
    {
        // Arrange
        var location1 = new LatLong((double)50, 5);
        var location2 = new LatLong((double)50, 5);

        // Act
        var result = new LatLongCalculator().CalculateDistanceBetween(location1, location2, MetresUnit);

        // Assert
        result.Should().Be(0);
    }

    private static DistanceUnit CreateDistanceUnit(
        double metres)
        => new DistanceUnit
        {
            Abbreviation = string.Empty,
            Metres = metres,
            Name = string.Empty,
            Type = DistanceUnitType.None
        };
}
