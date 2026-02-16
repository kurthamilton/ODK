using ODK.Core.Countries;

namespace ODK.Core.Venues;

public class VenueLocation : ILocation
{
    public double Latitude { get; set; }

    public LatLong LatLong => new LatLong(Latitude, Latitude);

    public double Longitude { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid VenueId { get; set; }
}