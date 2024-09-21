namespace ODK.Services.Topics.Models;

public class ApproveTopicsItemModel
{
    public required Guid NewTopicId { get; init; }

    public required string Topic { get; init; }

    public required string TopicGroup { get; init; }
}
