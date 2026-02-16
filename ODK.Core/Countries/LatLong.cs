using GeoCoordinatePortable;

namespace ODK.Core.Countries;

public struct LatLong
{
    public LatLong()
    {
    }

    public LatLong(double lat, double @long)
    {
        Lat = lat;
        Long = @long;
    }

    public bool IsDefault => Lat == 0 && Long == 0;

    public double Lat { get; set; }

    public double Long { get; set; }

    public static LatLong? FromCoords(double? lat, double? @long) 
        => lat != null && @long != null
            ? new LatLong(lat.Value, @long.Value)
            : null;

    public double DistanceFrom(LatLong other, DistanceUnit unit)
    {
        var (lat1, lon1) = (Lat, Long);
        var (lat2, lon2) = (other.Lat, other.Long);

        var coord1 = new GeoCoordinate(lat1, lon1);
        var coord2 = new GeoCoordinate(lat2, lon2);
        var distanceInMetres = coord1.GetDistanceTo(coord2);
        return distanceInMetres / unit.Metres;
    }

    public override string ToString() => $"{Lat},{Long}";
}