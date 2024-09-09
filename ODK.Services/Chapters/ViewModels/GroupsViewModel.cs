using ODK.Core.Countries;
using ODK.Core.Platforms;
using ODK.Core.Topics;

namespace ODK.Services.Chapters.ViewModels;

public class GroupsViewModel
{
    public required Distance Distance { get; init; }

    public required IReadOnlyCollection<DistanceUnit> DistanceUnits { get; init; }

    public required ILocation? Location { get; init; }    

    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Groups { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required Guid? TopicGroupId { get; init; }
}
