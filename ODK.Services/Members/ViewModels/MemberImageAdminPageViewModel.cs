using ODK.Core.Members;
using ODK.Data.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberImageAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required MemberAvatarVersionDto? Avatar { get; init; }

    public required MemberImageVersionDto? Image { get; init; }
}
