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

    public double Lat { get; set; }

    public double Long { get; set; }
}
