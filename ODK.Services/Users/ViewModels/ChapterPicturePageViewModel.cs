using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterPicturePageViewModel : SitePicturePageViewModel
{
    public required Chapter Chapter { get; init; }
}
