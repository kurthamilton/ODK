using ODK.Core.Topics;

namespace ODK.Services.Topics.ViewModels;

public class TopicAdminPageViewModel
{
    public required Topic Topic { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }
}