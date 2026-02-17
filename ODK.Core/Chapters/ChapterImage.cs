namespace ODK.Core.Chapters;

public class ChapterImage : IVersioned, IChapterEntity
{
    public const decimal AspectRatio = 16.0M / 9;

    public const string DefaultMimeType = "image/webp";

    public const int MaxWidth = 600;

    public Guid ChapterId { get; set; }

    public byte[] ImageData { get; set; } = [];

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];

    public int VersionInt { get; set; }
}