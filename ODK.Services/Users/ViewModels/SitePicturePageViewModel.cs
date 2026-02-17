using ODK.Core.Members;

namespace ODK.Services.Users.ViewModels;

public class SitePicturePageViewModel
{
    public required int? AvatarVersion { get; init; }

    public required Member CurrentMember { get; init; }

    public required int? ImageVersion { get; init; }
}