using ODK.Core.Topics;

namespace ODK.Core.Chapters;

public class ChapterTopic
{
    public Guid ChapterId { get; set; }

    public Topic Topic { get; set; } = null!;

    public Guid TopicId { get; set; }
}
