namespace ODK.Core.Topics;

public interface INewTopic
{
    Guid MemberId { get; }

    string Topic { get; }

    string TopicGroup { get; }
}
