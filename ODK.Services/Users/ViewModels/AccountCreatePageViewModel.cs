using ODK.Core.Topics;

namespace ODK.Services.Users.ViewModels;

public class AccountCreatePageViewModel
{
    public required string GoogleClientId { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
