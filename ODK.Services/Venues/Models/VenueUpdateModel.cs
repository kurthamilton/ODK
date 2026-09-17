namespace ODK.Services.Venues.Models;

/// <summary>
/// Everything a chapter may change about a venue it uses. The place itself is not here: it is what the
/// lookup said it was, and changing it means linking a different one.
/// </summary>
public class VenueUpdateModel
{
    public required string? AdditionalInfo { get; set; }

    /// <inheritdoc cref="VenueCreateModel.Name" path="/summary"/>
    public required string? Name { get; set; }
}
