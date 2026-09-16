namespace ODK.Core.Venues;

/// <summary>
/// Links a venue to a chapter that uses it. A venue is site-level, so several chapters can hold events
/// at the same one.
/// </summary>
public class ChapterVenue
{
    public Guid ChapterId { get; set; }

    public Guid VenueId { get; set; }
}
