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

    public static LatLong? FromCoords(double? lat, double? @long) => lat != null && @long != null
        ? new LatLong(lat.Value, @long.Value)
        : null;

    public double DistanceFrom(LatLong other, DistanceUnit unit)
    {
        var (lat1, lon1) = (Lat, Long);
        var (lat2, lon2) = (other.Lat, other.Long);

        // http://www.movable-type.co.uk/scripts/latlong.html
        // Haversine formula:
        // a = sin²(Δφ / 2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ / 2)
        // c = 2 ⋅ atan2( √a, √(1−a) )
        // d = R ⋅ c
        // where: 	φ is latitude, λ is longitude, R is earth’s radius (mean radius = 6,371km);
        // note that angles need to be in radians to pass to trig functions!
        const double R = 6_371_000;
        var t1 = lat1 * Math.PI / 180; // φ1, λ in radians
        var t2 = lat2 * Math.PI / 180; // φ2
        var dLat = (lat2 - lat1) * Math.PI / 180; // Δφ 
        var dLon = (lon2 - lon1) * Math.PI / 180; // Δλ

        var a = 
            Math.Pow(Math.Sin(dLat / 2), 2) +
            Math.Cos(t1) * Math.Cos(t2) *
            Math.Pow(Math.Sin(dLon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // in metres
        return d / unit.Metres;
    }

    public override string ToString() => $"{Lat},{Long}";
}
