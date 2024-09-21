namespace ODK.Services.Topics.Models;

public class NewTopicModel
{
    public required string Topic { get; init; }

    public required string TopicGroup { get; init; }

    public static IReadOnlyCollection<NewTopicModel> Build(
        IReadOnlyCollection<string?>? topicGroups,
        IReadOnlyCollection<string?>? topics)
    {
        var newTopics = new List<NewTopicModel>();
        if (topicGroups == null || topics == null)
        {
            return newTopics;
        }

        var count = Math.Min(topicGroups.Count, topics.Count);
        for (var i = 0; i < count; i++)
        {
            var topicGroup = topicGroups.ElementAt(i);
            var topic = topics.ElementAt(i);

            if (string.IsNullOrEmpty(topicGroup) || string.IsNullOrEmpty(topic))
            {
                continue;
            }

            newTopics.Add(new NewTopicModel
            {
                Topic = topic,
                TopicGroup = topicGroup
            });
        }

        return newTopics;
    }
}
