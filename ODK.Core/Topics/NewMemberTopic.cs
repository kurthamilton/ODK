namespace ODK.Core.Topics;

public class NewMemberTopic : IDatabaseEntity, INewTopic
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public required string Topic { get; set; }

    public required string TopicGroup { get; set; }
}
