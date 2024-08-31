using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPropertyAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> Options { get; init; }

    public required ChapterProperty Property { get; init; }    
}
