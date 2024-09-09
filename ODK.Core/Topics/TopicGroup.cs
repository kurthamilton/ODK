namespace ODK.Core.Topics;

public class TopicGroup : IDatabaseEntity
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }
}
