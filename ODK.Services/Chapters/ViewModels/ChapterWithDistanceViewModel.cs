using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Platforms;
using ODK.Core.Topics;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterWithDistanceViewModel
{
    public required Chapter Chapter { get; init; }

    public required Distance? Distance { get; init; }

    public required ChapterImageVersionDto? Image { get; init; }

    public required bool IsAdmin { get; init; }

    public required bool IsMember { get; init; }

    public required bool IsOwner { get; init; }

    public required ChapterLocation? Location { get; init; }

    public required PlatformType Platform { get; init; }

    public required string? ShortDescription { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}