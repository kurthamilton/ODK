using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Topics;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterTopicsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterTopic> ChapterTopics { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
