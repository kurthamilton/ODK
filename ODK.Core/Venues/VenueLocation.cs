using ODK.Core.Countries;

namespace ODK.Core.Venues;

public class VenueLocation : ILocation
{
    public decimal Latitude { get; set; }

    public LatLong LatLong => new LatLong(Latitude, Latitude);

    public decimal Longitude { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid VenueId { get; set; }

    public double DistanceFrom(ILocation other, DistanceUnit unit)
        => LatLong.DistanceFrom(other.LatLong, unit);
}
