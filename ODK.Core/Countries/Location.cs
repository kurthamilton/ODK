namespace ODK.Core.Countries;

public class Location : ILocation
{
    public required LatLong LatLong { get; init; }

    public required string Name { get; init; }
}