namespace ODK.Core.Venues;

public class Venue : IVersioned, IDatabaseEntity
{
    /// <summary>
    /// Matches the nvarchar(255) the database already uses for Chapters.Name, Chapters.Slug and
    /// Venues.Name, and keeps the unique index on Slug possible — nvarchar(max) cannot be indexed. Slugs
    /// longer than this are truncated before any version suffix is applied; slugifying can lengthen a
    /// name, since symbols expand to words ("&amp;" becomes " and ").
    /// </summary>
    public const int SlugMaxLength = 255;

    /// <summary>
    /// Matches the nvarchar(255) the database already uses for Venues.Name. Shared with
    /// <see cref="ChapterVenue.Name"/>, which holds a chapter's own name for the same venue and has to
    /// accept anything this one does.
    /// </summary>
    public const int NameMaxLength = 255;

    /// <summary>
    /// Used when a name has nothing sluggable at all (no letters or digits in any script). The slug is
    /// required, so there has to be something to fall back to; the usual version suffix keeps it unique.
    /// </summary>
    public const string SlugFallback = "venue";

    public string? Address { get; set; }

    /// <summary>
    /// When this record of the place was made. A venue is never rewritten - a place that changes name or
    /// moves becomes a new venue - so this is what orders the record of a place over time.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    public Guid Id { get; set; }

    public string? MapQuery { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-safe form of <see cref="Name"/>, unique across the site. Not a stable identifier — it
    /// changes when the venue is renamed; <see cref="Id"/> is canonical.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}