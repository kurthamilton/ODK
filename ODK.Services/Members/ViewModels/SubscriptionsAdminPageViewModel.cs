using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsAdminPageViewModel : SubscriptionsPageViewModel
{
    public required Chapter Chapter { get; init; }
    
    public required PlatformType Platform { get; init; }
}
