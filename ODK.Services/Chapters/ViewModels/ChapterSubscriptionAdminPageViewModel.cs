using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Subscriptions.ViewModels;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterSubscriptionAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required SiteSubscriptionsViewModel SiteSubscriptions { get; init; }
}
