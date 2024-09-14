using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberDeleteAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required MemberSubscription? MemberSubscription { get; init; }
}
