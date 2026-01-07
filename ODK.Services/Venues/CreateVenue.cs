using ODK.Core.Countries;

namespace ODK.Services.Venues;

public class CreateVenue
{
    public required string? Address { get; set; }

    public bool HasLocation => Location != null && !string.IsNullOrEmpty(LocationName);

    public required LatLong? Location { get; set; }

    public required string? LocationName { get; set; }

    public required string Name { get; set; } = string.Empty;
}
