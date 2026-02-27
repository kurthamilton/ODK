namespace ODK.Web.Razor.Models.Notifications;

public class ChapterNotificationSettingsFormViewModel
{
    public required string ChapterName { get; init; }

    public required Guid MemberChapterId { get; init; }

    public required List<NotificationSettingFormViewModel> Settings { get; set; } = new();
}