using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class MemberAdminPageViewModel
{
    public required Chapter Chapter { get; init; }    

    public required Member Member { get; init; }

    public required PlatformType Platform { get; init; }

    public required MemberSubscription? Subscription { get; init; }
}
