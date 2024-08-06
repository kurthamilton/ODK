using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfilePageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterProfileFormViewModel ChapterProfile { get; init; }

    public required Member CurrentMember { get; init; }
}
