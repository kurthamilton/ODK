using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel : GroupPageViewModelBase
{
    public required ChapterLocation? ChapterLocation { get; init; }

    public required int MemberCount { get; init; }
}
