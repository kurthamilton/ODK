namespace ODK.Web.Razor.Models.Notifications;

public class NotificationSettingsFormViewModel
{
    public required List<NotificationSettingFormViewModel> Settings { get; set; } = new();
}
