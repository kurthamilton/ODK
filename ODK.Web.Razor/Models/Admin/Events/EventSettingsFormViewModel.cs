using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel : EventSettingsFormSubmitViewModel
{
    public required Chapter Chapter { get; init; }

    public required bool ScheduledEmailsEnabled { get; init; }
}