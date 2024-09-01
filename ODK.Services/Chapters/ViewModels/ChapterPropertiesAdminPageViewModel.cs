using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPropertiesAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<ChapterProperty> Properties { get; init; }
}
