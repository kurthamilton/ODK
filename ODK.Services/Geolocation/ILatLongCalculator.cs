using ODK.Core.Countries;

namespace ODK.Services.Geolocation;

public interface ILatLongCalculator
{
    double CalculateDistanceBetween(LatLong x, LatLong y, DistanceUnit unit);
}