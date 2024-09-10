namespace ODK.Core.Topics;

public class Topic : IDatabaseEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required bool Public { get; set; }

    public TopicGroup TopicGroup { get; set; } = null!;

    public required Guid TopicGroupId { get; set; }
}
