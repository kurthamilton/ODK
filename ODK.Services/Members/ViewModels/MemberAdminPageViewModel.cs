using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberAdminPageViewModel : MemberAdminPageViewModelBase
{
    public required MemberSubscription? Subscription { get; init; }
}
