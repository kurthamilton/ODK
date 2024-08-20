using ODK.Core.Countries;

namespace ODK.Core.Venues;

public class VenueLocation
{
    public LatLong LatLong { get; set; }

    public string Name { get; set; } = "";

    public Guid VenueId { get; set; }
}
