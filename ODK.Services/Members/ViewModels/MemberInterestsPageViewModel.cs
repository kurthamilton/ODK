using ODK.Core.Members;
using ODK.Core.Topics;

namespace ODK.Services.Members.ViewModels;
public class MemberInterestsPageViewModel
{
    public required IReadOnlyCollection<MemberTopic> MemberTopics { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
