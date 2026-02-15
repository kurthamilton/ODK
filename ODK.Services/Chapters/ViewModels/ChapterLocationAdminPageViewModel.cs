using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterLocationAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Country? Country { get; init; }

    public required ChapterLocationDto? Location { get; init; }
}