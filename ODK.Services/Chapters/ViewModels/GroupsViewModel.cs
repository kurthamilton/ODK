using ODK.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class GroupsViewModel
{
    public required Distance Distance { get; init; }

    public required ILocation Location { get; init; }    

    public required IReadOnlyCollection<ChapterWithDistanceViewModel> Groups { get; init; }
}
