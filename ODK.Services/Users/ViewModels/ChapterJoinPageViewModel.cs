using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterJoinPageViewModel
{
    public required string ChapterName { get; init; }    

    public required ChapterProfileFormViewModel Profile { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
