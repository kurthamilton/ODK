using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberImageAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required MemberAvatar? Avatar { get; init; }

    public required MemberImage? Image { get; init; }
}
