using ODK.Core.Topics;

namespace ODK.Core.Chapters;

public class ChapterTopicDto
{
    public required Guid ChapterId { get; init; }

    public required Topic Topic { get; init; }
}
