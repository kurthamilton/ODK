using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class SitePicturePageViewModel
{
    public required MemberAvatar? Avatar { get; init; }

    public required Member CurrentMember { get; init; }

    public required MemberImage? Image { get; init; }
}
