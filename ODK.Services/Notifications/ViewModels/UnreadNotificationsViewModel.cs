using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Notifications;

namespace ODK.Services.Notifications.ViewModels;

public class UnreadNotificationsViewModel
{
    public required Member CurrentMember { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<NotificationDto> Unread { get; init; }
}