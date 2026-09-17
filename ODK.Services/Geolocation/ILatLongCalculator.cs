using ODK.Core.Countries;

namespace ODK.Services.Geolocation;

public interface ILatLongCalculator
{
    double CalculateDistanceBetween(LatLong x, LatLong y, DistanceUnit unit);

    /// <summary>
    /// The distance in metres, which is what the calculation produces before a unit scales it. For a
    /// comparison against a threshold, where naming a unit would only mean naming metres.
    /// </summary>
    double CalculateMetresBetween(LatLong x, LatLong y);
}