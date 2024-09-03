using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class ChapterAccountViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member CurrentMember { get; init; }
}
