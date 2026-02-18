using ODK.Data.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberImageAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required int? AvatarVersion { get; init; }
}
