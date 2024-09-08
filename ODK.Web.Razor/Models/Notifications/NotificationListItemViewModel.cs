using ODK.Core.Notifications;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Notifications;

public class NotificationListItemViewModel
{
    public required Notification Notification { get; init; }

    public required PlatformType Platform { get; init; }
}
