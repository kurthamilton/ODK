using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLocationAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Country? Country { get; init; }

    public required ChapterLocation? Location { get; init; }

    public required PlatformType Platform { get; init; }        
}
