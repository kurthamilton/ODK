using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterAboutPageViewModel
{
    public required ChapterPage? ChapterPage { get; init; }

    public required IReadOnlyCollection<ChapterQuestion> Questions { get; init; }

    public required ChapterTexts? Texts { get; init; }
}