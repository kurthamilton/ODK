using ODK.Core.Notifications;
using ODK.Data.Core.Notifications;

namespace ODK.Web.Razor.Models.Notifications;

public class NotificationListItemViewModel
{
    public required NotificationDto Notification { get; init; }
}