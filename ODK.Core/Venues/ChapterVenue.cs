namespace ODK.Core.Venues;

/// <summary>
/// Links a venue to a chapter that uses it. A venue is site-level, so several chapters can hold events
/// at the same one.
/// </summary>
public class ChapterVenue
{
    /// <summary>
    /// When this chapter archived the venue. Archiving is per chapter: a venue one chapter has finished
    /// with is untouched for every other chapter using it.
    /// </summary>
    public DateTime? ArchivedUtc { get; set; }

    public Guid ChapterId { get; set; }

    /// <summary>
    /// This chapter's own name for the venue, overriding <see cref="Venue.Name"/>. Null means the
    /// chapter uses the site-level name.
    /// </summary>
    public string? Name { get; set; }

    public required Venue Venue { get; set; } = null!;

    public Guid VenueId { get; set; }
}
