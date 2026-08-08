namespace ODK.Core.Venues;

public class Venue : IVersioned, IDatabaseEntity, IChapterEntity
{
    /// <summary>
    /// Matches the nvarchar(255) the database already uses for Chapters.Name, Chapters.Slug and
    /// Venues.Name, and keeps a unique index on (ChapterId, Slug) possible — nvarchar(max) cannot be
    /// indexed. Slugs longer than this are truncated before any version suffix is applied; slugifying
    /// can lengthen a name, since symbols expand to words ("&amp;" becomes " and ").
    /// </summary>
    public const int SlugMaxLength = 255;

    public string? Address { get; set; }

    public DateTime? ArchivedUtc { get; set; }

    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public string? MapQuery { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-safe form of <see cref="Name"/>, unique within the chapter. Nullable while it is being
    /// rolled out. Not a stable identifier — it changes when the venue is renamed; <see cref="Id"/>
    /// is canonical.
    /// </summary>
    public string? Slug { get; set; }

    public byte[] Version { get; set; } = [];
}