using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterAccountPageViewModel : SiteAccountPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterProfileFormViewModel ChapterProfile { get; init; }
}
