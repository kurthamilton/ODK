using ODK.Core.Chapters;
using ODK.Core.Subscriptions;

namespace ODK.Services.Events.ViewModels;

public class EventSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public required ChapterEventSettings? Settings { get; init; }
}
