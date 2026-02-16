namespace ODK.Data.Core.Chapters;

public class ChapterImageMetadataDto
{
    public required Guid ChapterId { get; init; }

    public required string MimeType { get; init; }
}