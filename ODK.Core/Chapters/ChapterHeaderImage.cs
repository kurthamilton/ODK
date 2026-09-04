namespace ODK.Core.Chapters;

/// <summary>
/// The image in the header of a chapter's pages. Deliberately carries no aspect ratio or maximum width,
/// unlike <see cref="ChapterImage"/>: it is uploaded by a site admin, who is the person sizing it.
/// </summary>
public class ChapterHeaderImage : IVersioned, IChapterEntity
{
    public const string DefaultMimeType = "image/webp";

    public Guid ChapterId { get; set; }

    public byte[] ImageData { get; set; } = [];

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];

    public int VersionInt { get; set; }
}
