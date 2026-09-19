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

    /// <summary>
    /// The pair as "lat,long", which is what the Maps embed takes and what the location picker's script
    /// reads back.
    /// </summary>
    /// <remarks>
    /// Invariant, and not the request's culture: a locale whose decimal separator is a comma writes
    /// "53,3811,-1,4701", which is four values to anything splitting on commas and is a coordinate no
    /// reader recovers. The script at the other end writes plain JavaScript numbers, so this is the
    /// spelling both ends have to agree on.
    /// </remarks>
    public override string ToString() => FormattableString.Invariant($"{Lat},{Long}");
}