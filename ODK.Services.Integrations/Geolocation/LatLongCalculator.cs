using GeoCoordinatePortable;
using ODK.Core.Countries;
using ODK.Services.Geolocation;

namespace ODK.Services.Integrations.Geolocation;

public class LatLongCalculator : ILatLongCalculator
{
    public double CalculateDistanceBetween(LatLong x, LatLong y, DistanceUnit unit)
        => CalculateMetresBetween(x, y) / unit.Metres;

    public double CalculateMetresBetween(LatLong x, LatLong y)
    {
        var coordX = new GeoCoordinate(x.Lat, x.Long);
        var coordY = new GeoCoordinate(y.Lat, y.Long);
        return coordX.GetDistanceTo(coordY);
    }
}