namespace ODK.Core.Countries;

public class Location : ILocation
{
    public required LatLong LatLong { get; init; }

    public required string Name { get; init; }

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}
