using ODK.Core.Topics;

namespace ODK.Services.Topics.ViewModels;

public class TopicsAdminPageViewModel
{
    public required IReadOnlyCollection<NewChapterTopic> NewChapterTopics { get; init; }

    public required IReadOnlyCollection<NewMemberTopic> NewMemberTopics { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
