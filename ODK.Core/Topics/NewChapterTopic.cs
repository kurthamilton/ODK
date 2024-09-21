namespace ODK.Core.Topics;

public class NewChapterTopic : IDatabaseEntity, INewTopic
{
    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public required string Topic { get; set; }

    public required string TopicGroup { get; set; }
}
