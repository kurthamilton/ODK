using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Events.ViewModels;

public class EventSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required ChapterEventSettings? Settings { get; init; }
}
