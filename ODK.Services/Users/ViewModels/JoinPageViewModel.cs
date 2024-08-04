using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class JoinPageViewModel
{
    public required string ChapterName { get; init; }    

    public required ProfileFormViewModel Profile { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
