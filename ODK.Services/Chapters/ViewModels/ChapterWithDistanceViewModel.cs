using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Topics;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterWithDistanceViewModel
{
    public required Chapter Chapter { get; init; }

    public required double Distance { get; init; }

    public required ChapterLocation Location { get; init; }

    public required PlatformType Platform { get; init; }

    public required ChapterTexts? Texts { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}
