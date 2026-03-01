namespace ODK.Web.Razor.Models.Notifications;

public class NotificationSettingsFormViewModel : NotificationSettingsSubmitFormModel
{
    public required IReadOnlyCollection<Guid> AdminChapterIds { get; init; }
}