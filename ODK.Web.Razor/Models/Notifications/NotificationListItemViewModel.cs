using ODK.Core.Notifications;

namespace ODK.Web.Razor.Models.Notifications;

public class NotificationListItemViewModel
{
    public required Notification Notification { get; init; }
}