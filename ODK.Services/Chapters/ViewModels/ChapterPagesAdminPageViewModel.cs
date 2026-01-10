using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPagesAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterPage> ChapterPages { get; init; }
}