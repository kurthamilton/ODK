using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLayoutViewModel
{
    public required ChapterLinks? Links { get; init; }

    public required IReadOnlyCollection<ChapterPage> Pages { get; init; }
}