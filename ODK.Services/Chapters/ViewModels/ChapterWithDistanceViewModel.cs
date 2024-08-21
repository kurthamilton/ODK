using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterWithDistanceViewModel
{
    public required Chapter Chapter { get; init; }

    public required double Distance { get; init; }

    public required ChapterLocation Location { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
