namespace ODK.Data.Core.Chapters;

public class ChapterImageVersionDto
{
    /// <summary>
    /// The chapter the image belongs to. Only meaningful to a caller that asked about several chapters at
    /// once, but carried always so a batched result can be matched back up.
    /// </summary>
    public required Guid ChapterId { get; init; }

    public required int Version { get; init; }
}