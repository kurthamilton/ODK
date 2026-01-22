namespace ODK.Core.Topics;

public class TopicGroup : IDatabaseEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}