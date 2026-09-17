namespace ODK.Core.Venues;

/// <summary>
/// Links a venue to a chapter that uses it. A venue is site-level, so several chapters can hold events
/// at the same one.
/// </summary>
public class ChapterVenue
{
    /// <summary>
    /// Longer than the site-level text columns because this is prose rather than a label - directions run
    /// to a paragraph, and 4000 is as far as nvarchar goes before it stops being indexable.
    /// </summary>
    public const int AdditionalInfoMaxLength = 4000;

    /// <summary>
    /// When this chapter archived the venue. Archiving is per chapter: a venue one chapter has finished
    /// with is untouched for every other chapter using it.
    /// </summary>
    public DateTime? ArchivedUtc { get; set; }

    /// <summary>
    /// This chapter's note about reaching the venue - which door, who to ask for. A property of how the
    /// chapter uses the place rather than of the place.
    /// </summary>
    public string? AdditionalInfo { get; set; }

    public Guid ChapterId { get; set; }

    /// <summary>
    /// This chapter's own name for the venue, overriding <see cref="Venue.Name"/>. Null means the
    /// chapter uses the site-level name.
    /// </summary>
    public string? Name { get; set; }

    public required Venue Venue { get; set; }

    public Guid VenueId { get; set; }
}
