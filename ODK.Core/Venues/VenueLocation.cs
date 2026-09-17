using ODK.Core.Countries;

namespace ODK.Core.Venues;

public class VenueLocation : ILocation
{
    /// <summary>
    /// Google does not document a maximum length for a place ID and says not to assume one, so this is
    /// simply long enough to be untroubled by the ones it issues, which run to about 30 characters.
    /// </summary>
    public const int ExternalIdMaxLength = 255;

    /// <summary>
    /// The Google place ID this location came from. It sits here rather than on the venue because it and
    /// the coordinates arrive from one lookup and describe one thing: where the place is. Null for a
    /// location recorded before the lookup became the source, and for one no place could be matched to.
    /// </summary>
    public string? ExternalId { get; set; }

    public double Latitude { get; set; }

    public LatLong LatLong => new LatLong(Latitude, Longitude);

    public double Longitude { get; set; }

    public string? MapQuery { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid VenueId { get; set; }
}