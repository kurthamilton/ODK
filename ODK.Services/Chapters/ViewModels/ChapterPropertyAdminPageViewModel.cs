using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPropertyAdminPageViewModel
{
    public required IReadOnlyCollection<ChapterPropertyOption> Options { get; init; }

    public required ChapterProperty Property { get; init; }
}
