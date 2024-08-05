using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class AccountPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member CurrentMember { get; init; }

    public required MemberAvatar? Avatar { get; init; }

    public required ChapterProfileFormViewModel Profile { get; init; }
}
