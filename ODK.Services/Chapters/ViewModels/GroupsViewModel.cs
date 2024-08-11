using ODK.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class GroupsViewModel
{
    public required LatLong Location { get; init; }

    public required string LocationName { get; init; }

    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Groups { get; init; }
}
