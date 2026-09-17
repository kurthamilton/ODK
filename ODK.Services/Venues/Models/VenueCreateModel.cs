namespace ODK.Services.Venues.Models;

public class VenueCreateModel
{
    public required string? AdditionalInfo { get; set; }

    /// <summary>
    /// Identifies the place in the lookup the picker searched. The only field a venue cannot be created
    /// without: everything else about the place comes from resolving it.
    /// </summary>
    public required string? ExternalId { get; set; }

    /// <summary>
    /// What this chapter calls the venue. Empty, or the same as the place's own name, means it takes the
    /// name the lookup returned.
    /// </summary>
    public required string? Name { get; set; }
}
