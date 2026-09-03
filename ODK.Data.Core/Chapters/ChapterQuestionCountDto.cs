namespace ODK.Data.Core.Chapters;

public class ChapterQuestionCountDto
{
    /// <summary>
    /// The chapter the count belongs to. Only meaningful to a caller that asked about several chapters at
    /// once, but carried always so a batched result can be matched back up.
    /// </summary>
    public required Guid ChapterId { get; init; }

    public required int Count { get; init; }
}
