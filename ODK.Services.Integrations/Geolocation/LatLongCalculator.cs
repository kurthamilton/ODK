using GeoCoordinatePortable;
using ODK.Core.Countries;
using ODK.Services.Geolocation;

namespace ODK.Services.Integrations.Geolocation;

public class LatLongCalculator : ILatLongCalculator
{
    public double CalculateDistanceBetween(LatLong x, LatLong y, DistanceUnit unit)
    {
        var coordX = new GeoCoordinate(x.Lat, x.Long);
        var coordY = new GeoCoordinate(y.Lat, y.Long);
        var distanceInMetres = coordX.GetDistanceTo(coordY);
        return distanceInMetres / unit.Metres;
    }
}