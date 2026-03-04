using ODK.Core.Chapters;

namespace ODK.Services.Events.ViewModels;

public class EventSettingsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required bool ScheduledEmailsEnabled { get; init; }

    public required ChapterEventSettings? Settings { get; init; }
}