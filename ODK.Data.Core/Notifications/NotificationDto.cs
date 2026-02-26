using ODK.Core.Chapters;
using ODK.Core.Notifications;

namespace ODK.Data.Core.Notifications;

public class NotificationDto
{
    public required Chapter? Chapter { get; init; }

    public required Notification Notification { get; init; }
}