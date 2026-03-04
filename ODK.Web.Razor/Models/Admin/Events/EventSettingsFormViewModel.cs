namespace ODK.Web.Razor.Models.Admin.Events;

public class EventSettingsFormViewModel : EventSettingsFormSubmitViewModel
{
    public required bool ScheduledEmailsEnabled { get; init; }
}