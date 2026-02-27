namespace ODK.Web.Razor.Models.Notifications;

public class NotificationSettingsFormViewModel
{
    public required List<ChapterNotificationSettingsFormViewModel> ChapterSettings { get; init; } = [];

    public required List<NotificationSettingFormViewModel> Settings { get; init; } = [];
}