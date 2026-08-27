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

    public override string ToString() => $"{Lat},{Long}";
}