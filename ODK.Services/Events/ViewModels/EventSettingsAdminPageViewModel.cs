using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Events.ViewModels;

public class EventSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }

    public required ChapterEventSettings? Settings { get; init; }
}
