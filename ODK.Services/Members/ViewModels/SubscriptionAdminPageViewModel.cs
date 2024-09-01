using ODK.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionAdminPageViewModel : SubscriptionCreateAdminPageViewModel
{
    public required ChapterSubscription Subscription { get; init; }
}
