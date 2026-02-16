namespace ODK.Core.Countries;

public interface ILocation
{
    LatLong LatLong { get; }

    string Name { get; }
}