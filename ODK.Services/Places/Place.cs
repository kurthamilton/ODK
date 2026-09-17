using ODK.Core.Countries;

namespace ODK.Services.Places;

/// <summary>
/// A place as Google described it at the moment it was looked up. A venue is a record of one of these,
/// which is why nothing here is refreshed in place - see <see cref="IPlacesService"/>.
/// </summary>
public class Place
{
    public required string? FormattedAddress { get; init; }

    /// <summary>
    /// Identifies this place in the service it came from. Stable enough to key on and safe to store, but
    /// not immutable: a place that closes, moves or is merged can be given a new one, and the old one
    /// stops resolving.
    /// </summary>
    public required string ExternalId { get; init; }

    public required LatLong Location { get; init; }

    /// <summary>
    /// The town or city, taken from the address components. Null when Google returns none, which happens
    /// for places too rural or too remote to sit inside one.
    /// </summary>
    public required string? Locality { get; init; }

    public required string Name { get; init; }
}
